use crate::{
    game::{GamePlayer, models::PlayerRemovalResult},
    info::MOTD,
    messages::{
        EncapsulatedMessage, FetchGamesStatus, IncomingMessage, OutgoingMessage, ReconnectStatus,
        RoomCreateStatus, ServerMessage, SubmissionResult,
    },
    mods::check_mods,
    rooms::models::JoinEligibility,
    server::HttpState,
    util::CLIENT_VERSION,
};
use anyhow::Result;
use axum::{
    body::Bytes,
    extract::{
        ConnectInfo, State, WebSocketUpgrade,
        ws::{CloseFrame, Message, WebSocket},
    },
    response::Response,
};
use base64::{Engine, prelude::BASE64_STANDARD};
use crossbeam::channel::{Receiver, Sender, unbounded};
use futures_util::{
    SinkExt, StreamExt,
    stream::{SplitSink, SplitStream},
};
use std::{net::SocketAddr, sync::Arc};

#[axum::debug_handler]
pub async fn handle_upgrade(
    State(state): State<HttpState>,
    ConnectInfo(addr): ConnectInfo<SocketAddr>,
    ws: WebSocketUpgrade,
) -> Response {
    ws.on_upgrade(async move |socket| {
        if let Err(err) = handle_socket(socket, state, addr.ip().to_string()).await {
            error!("An error occured: {err}");
            error!("{}", err.backtrace());
        }
    })
}

async fn handle_socket(socket: WebSocket, state: HttpState, ip: String) -> Result<()> {
    info!("Client has connected!");

    let (writer, reader) = socket.split();
    let (tx, rx) = unbounded::<ServerMessage>();
    let local_tx = tx.clone();

    let write_task = tokio::task::spawn(write(writer, rx, Arc::clone(&state)));
    let read_task = tokio::task::spawn(read(reader, local_tx, state, ip));

    let write_abort = write_task.abort_handle();
    let read_abort = read_task.abort_handle();

    tokio::select! {
        _ = write_task => { read_abort.abort() }
        _ = read_task => { write_abort.abort() }
    };

    Ok(())
}

fn send(tx: &Sender<ServerMessage>, msg: OutgoingMessage) -> Result<()> {
    tx.send(ServerMessage::Normal(EncapsulatedMessage::new(msg)?))?;

    Ok(())
}

async fn process_message(
    data: String,
    state: &HttpState,
    tx: &Sender<ServerMessage>,
    ip: &String,
) -> Result<bool> {
    debug!("Got data: {data}");

    match BASE64_STANDARD.decode(&data) {
        Ok(decoded) => match serde_json::from_slice::<IncomingMessage>(&decoded) {
            Ok(data) => {
                debug!("Received data: {data:?}");

                let copy = Arc::clone(&state);
                let mut state = state.upgradable_read();

                match data {
                    IncomingMessage::CheatActivation {
                        game_id,
                        username: _,
                        steam_id,
                    } => {
                        state.humiliate_player(game_id, steam_id.parse()?)?;
                    }

                    IncomingMessage::ClearTeams { game_id, ticket } => {
                        if state.verify_connection(&ticket, true)? {
                            if let Some(game) = state.current_games.get(&game_id) {
                                game.write().clear_teams()?;
                            } else {
                                error!(
                                    "Failed to clear teams for game {game_id}, as it doesn't exist!"
                                );
                            }
                        }
                    }

                    // All these params aren't used... bruh
                    // Maybe they should be in a future update :P
                    IncomingMessage::CreateRoom {
                        room_name: _,
                        room_password: _,
                        max_players: _,
                        p_rank_required: _,
                        host_steam_name,
                        host_steam_id,
                        rank,
                    } => {
                        if state.rooms.check_ban(host_steam_id.parse()?, ip)? {
                            warn!("SteamID {host_steam_id} or IP address is banned from the mod!");

                            send(
                                tx,
                                OutgoingMessage::CreateRoomResponse {
                                    status: RoomCreateStatus::Ban,
                                    room_id: -1,
                                    room_details: None,
                                    room_password: String::new(),
                                },
                            )?;

                            return Ok(false);
                        }

                        info!("Creating new game in DB");

                        let steam_id = host_steam_id.parse()?;

                        match state.rooms.create(steam_id) {
                            Ok(room) => {
                                let game = state.with_upgraded(|state| {
                                    state.create_game(
                                        room.id,
                                        GamePlayer {
                                            username: host_steam_name,
                                            steam_id,
                                            rank,
                                            team: None,
                                            conn: tx.clone(),
                                        },
                                    )
                                })?;

                                send(
                                    tx,
                                    OutgoingMessage::CreateRoomResponse {
                                        status: RoomCreateStatus::Ok,
                                        room_id: room.id,
                                        room_details: Some(game.read().info()),
                                        room_password: room.room_code,
                                    },
                                )?;
                            }

                            Err(_) => {
                                error!("Failed to create room!");

                                send(
                                    tx,
                                    OutgoingMessage::CreateRoomResponse {
                                        status: RoomCreateStatus::Err,
                                        room_id: -1,
                                        room_details: None,
                                        room_password: String::new(),
                                    },
                                )?;
                            }
                        }
                    }

                    IncomingMessage::FetchGames => {
                        let games = state.rooms.get_public_games()?;

                        send(
                            tx,
                            OutgoingMessage::FetchGamesResponse {
                                status: if games.is_empty() {
                                    FetchGamesStatus::None
                                } else {
                                    FetchGamesStatus::Ok
                                },
                                games: serde_json::to_string(&games)?,
                            },
                        )?;
                    }

                    IncomingMessage::JoinRoom {
                        password,
                        username,
                        steam_id,
                        rank,
                    } => {
                        info!(
                            "Player {username} ({steam_id}) attempting to join game with password '{password}'"
                        );

                        let steam_id = steam_id.parse()?;

                        if let Ok(mut room) = state.rooms.find_game(password) {
                            let status =
                                room.check_join_eligibility(&steam_id, Some(ip.clone()))?;

                            match status {
                                JoinEligibility::Ok => {
                                    if let Some(game) = state.current_games.get(&room.room.id) {
                                        state.join_game(
                                            room.room.id,
                                            GamePlayer {
                                                steam_id,
                                                username,
                                                rank,
                                                team: None,
                                                conn: tx.clone(),
                                            },
                                        )?;

                                        send(
                                            tx,
                                            OutgoingMessage::JoinRoomResponse {
                                                status,
                                                room_id: room.room.id,
                                                room_details: Some(game.read().info()),
                                            },
                                        )?;
                                    } else {
                                        error!(
                                            "Failed to join player {username} ({steam_id}) to room {}: room exists but the game state does not!",
                                            room.room.id
                                        );
                                    }
                                }

                                other => {
                                    send(
                                        tx,
                                        OutgoingMessage::JoinRoomResponse {
                                            status: other,
                                            room_id: -1,
                                            room_details: None,
                                        },
                                    )?;

                                    return Ok(false);
                                }
                            }
                        } else {
                            send(
                                tx,
                                OutgoingMessage::JoinRoomResponse {
                                    status: JoinEligibility::NotFound,
                                    room_id: -1,
                                    room_details: None,
                                },
                            )?;

                            return Ok(false);
                        }
                    }

                    IncomingMessage::KickPlayer {
                        game_id,
                        player_to_kick,
                        ticket,
                    } => {
                        if state.verify_connection(&ticket, true)? {
                            state.kick_player(game_id, player_to_kick.parse()?)?;
                        }
                    }

                    IncomingMessage::LeaveGame {
                        room_id,
                        username,
                        steam_id,
                    } => {
                        info!("Player wants to leave game {room_id}");

                        let Some(username) = username else {
                            error!("Given username was None, aborting!");
                            return Ok(true);
                        };

                        match state.check_player_before_removing(room_id, steam_id.parse()?) {
                            PlayerRemovalResult::GameNotFound
                            | PlayerRemovalResult::PlayerNotFound => {
                                error!("Unable to remove the specified player!")
                            }

                            PlayerRemovalResult::IsHost => {
                                state
                                    .disconnect_all_players(room_id, Some("HOSTLEFTGAME".into()))?;
                                state.with_upgraded(|state| state.destroy_game(room_id))?;
                            }

                            PlayerRemovalResult::Ok => {
                                state.disconnect_player(room_id, username, steam_id.parse()?)?;
                            }
                        }
                    }

                    IncomingMessage::MapPing {
                        game_id,
                        team,
                        row,
                        column,
                        ticket,
                    } => {
                        if state.verify_connection(&ticket, false)? {
                            if state.current_games.contains_key(&game_id) {
                                state.map_ping(game_id, team, row, column)?;
                            }
                        }
                    }

                    IncomingMessage::ReconnectRequest {
                        room_id,
                        steam_id,
                        ticket,
                    } => {
                        let steam_id = steam_id.parse()?;

                        info!("Player requesting reconnection");

                        if !state.verify_connection(&ticket, false)? {
                            warn!(
                                "Rejecting reconnection: invalid Steam ticket, or player is not in game"
                            );
                            return Ok(true);
                        }

                        if let Some(game) = state.current_games.get(&room_id) {
                            let mut game = game.write();

                            if let Some(player) = game.current_players.get_mut(&steam_id) {
                                player.conn = tx.clone();
                                state.update_connection(&steam_id, ip.clone(), room_id)?;

                                info!("Sending fresh game data to reconnected player");

                                send(
                                    tx,
                                    OutgoingMessage::ReconnectResponse {
                                        status: ReconnectStatus::Ok,
                                        game_data: Some(game.info()),
                                    },
                                )?;
                            }
                        } else {
                            error!("Game no longer exists in coordinator, sending error response");

                            send(
                                tx,
                                OutgoingMessage::ReconnectResponse {
                                    status: ReconnectStatus::End,
                                    game_data: None,
                                },
                            )?;
                        }
                    }

                    IncomingMessage::RegisterTicket(ticket) => {
                        state.connect(
                            ip,
                            ticket.steam_ticket,
                            &ticket.steam_id.parse()?,
                            ticket.steam_username,
                            ticket.game_id,
                        )?;
                    }

                    IncomingMessage::RerollRequest {
                        steam_ticket: ticket,
                        steam_id,
                        game_id,
                        row: x,
                        column: y,
                    } => {
                        let steam_id = steam_id.parse()?;

                        if state.verify_connection(&ticket, false)? {
                            if let Some(game) = state.current_games.get(&game_id) {
                                let mut game = game.upgradable_read();

                                info!("Requesting reroll of map at [{x}, {y}] in game {game_id}");

                                if game.is_vote_active {
                                    if !game.has_player_voted(steam_id) {
                                        game.with_upgraded(|game| game.add_player_vote(steam_id))?;
                                    }
                                } else {
                                    info!("Vote not active in game, starting");

                                    if game.can_player_start_vote(steam_id) {
                                        game.with_upgraded(|game| {
                                            game.start_reroll_vote(&copy, steam_id, x, y)
                                        })?;
                                    }
                                }
                            }
                        }
                    }

                    IncomingMessage::UpdateRoomSettings(data) => {
                        if state.verify_connection(&data.ticket, true)? {
                            info!("Updating settings for room {}", data.room_id);

                            state.update_game_settings(data)?;
                        }
                    }

                    IncomingMessage::StartGame { room_id, ticket } => {
                        if state.verify_connection(&ticket, true)? {
                            info!("Starting game {room_id}!");

                            state.start_game(room_id, &copy)?;
                        }
                    }

                    IncomingMessage::SubmitRun(data) => {
                        // let steam_id = steam_id.parse()?;

                        if !state.verify_connection(&data.ticket, false)? {
                            warn!(
                                "Rejecting submission: invalid Steam ticket, or player is not in game"
                            );
                            return Ok(true);
                        }

                        if state.verify_run_submission(&data) {
                            match state.submit_run(data.clone())? {
                                SubmissionResult::CriteriaFailed => (),

                                result => {
                                    if let Some(game) = state.current_games.get(&data.game_id) {
                                        if let Ok(mut room) = state.rooms.get_game(data.game_id) {
                                            let mut game = game.upgradable_read();
                                            let voting =
                                                game.vote_pos == Some([data.row, data.column]);

                                            if voting {
                                                info!(
                                                    "Claimed map is being voted on, cancelling vote"
                                                );
                                                game.with_upgraded(|game| {
                                                    game.reset_vote_variables()
                                                });
                                            }

                                            let mut info = game.info();

                                            game.with_upgraded(|game| -> Result<()> {
                                                game.mode.on_map_claim(
                                                    &mut info, &mut room, data, result, voting,
                                                )?;
                                                game.apply(info);

                                                Ok(())
                                            })?;
                                        } else {
                                            error!(
                                                "Failed to complete run submission for game {}, as its room couldn't be fetched!",
                                                data.game_id
                                            );
                                        }
                                    } else {
                                        error!(
                                            "Failed to complete run submission for game {}, as it doesn't exist!",
                                            data.game_id
                                        );
                                    }
                                }
                            }
                        }
                    }

                    IncomingMessage::UpdateTeamSettings {
                        game_id,
                        teams,
                        ticket,
                    } => {
                        if state.verify_connection(&ticket, true)? {
                            if let Some(game) = state.current_games.get(&game_id) {
                                info!("Updating teams for game {game_id}");

                                let names = game.read().team_names.clone();

                                // TODO: Make it possible to have custom team names

                                game.write().update_teams(
                                    teams
                                        .into_iter()
                                        .map(|(steam_id, team_idx)| {
                                            (steam_id.parse().unwrap(), names[team_idx].clone())
                                        })
                                        .collect(),
                                )?;
                            } else {
                                error!(
                                    "Failed to update teams for game {game_id}, as it doesn't exist!"
                                );
                            }
                        }
                    }

                    IncomingMessage::UpdateMapPool {
                        game_id,
                        map_pool_ids,
                        ticket,
                    } => {
                        if state.verify_connection(&ticket, true)? {
                            if let Some(game) = state.current_games.get(&game_id) {
                                info!("Updating map pools for game {game_id}");
                                game.write().update_map_pool(map_pool_ids);
                            } else {
                                error!(
                                    "Failed to update map pools for game {game_id}, as it doesn't exist!"
                                );
                            }
                        }
                    }

                    IncomingMessage::VerifyModList {
                        client_mod_list,
                        steam_id,
                    } => {
                        let unauthorized = check_mods(&steam_id, client_mod_list);
                        let ranks = state.rooms.fetch_available_ranks(steam_id.parse()?)?;

                        if ranks.is_empty() {
                            info!("No ranks for requesting SteamID");
                        }

                        send(
                            tx,
                            OutgoingMessage::ModVerificationResponse {
                                non_whitelisted_mods: unauthorized,
                                latest_version: CLIENT_VERSION.into(),
                                motd: MOTD.into(),
                                available_ranks: ranks.join(","),
                            },
                        )?;
                    }
                }
            }

            Err(err) => {
                warn!(
                    "Got {} bytes of garbage/unparseable data!\nError: {err}",
                    data.as_bytes().len()
                );

                debug!("Garbage data: {data}");
            }
        },

        Err(err) => {
            warn!(
                "Failed to decode {} bytes of base64!\nError: {err}",
                data.as_bytes().len()
            );
            debug!("Non-decodable data: {data}");
        }
    };

    Ok(true)
}

async fn read(
    mut reader: SplitStream<WebSocket>,
    tx: Sender<ServerMessage>,
    state: HttpState,
    ip: String,
) -> Result<()> {
    let mut disconnected = false;

    while let Some(Ok(msg)) = reader.next().await {
        debug!("Got message: {:?}", msg);

        match msg {
            Message::Ping(_) => tx.send(ServerMessage::Pong)?,

            Message::Close(_) => {
                info!("Client has disconnected!");
                tx.send(ServerMessage::ForceClose)?;
                disconnected = true;
                break;
            }

            Message::Text(data) => {
                if !process_message(data.to_string(), &state, &tx, &ip).await? {
                    info!("Client has disconnected!");
                    tx.send(ServerMessage::ForceClose)?;
                    disconnected = true;
                    break;
                }
            }

            Message::Binary(data) => {
                if let Ok(data) = String::from_utf8(data.to_vec()) {
                    if !process_message(data, &state, &tx, &ip).await? {
                        info!("Client has disconnected!");
                        tx.send(ServerMessage::ForceClose)?;
                        disconnected = true;
                        break;
                    }
                }
            }

            Message::Pong(_) => {
                debug!("Got pong!");
            }
        }
    }

    if !disconnected {
        info!("Client has disconnected!");
        tx.send(ServerMessage::ForceClose)?;
    }

    Ok(())
}

async fn write(
    mut writer: SplitSink<WebSocket, Message>,
    rx: Receiver<ServerMessage>,
    _state: HttpState,
) -> Result<()> {
    while let Ok(msg) = rx.recv() {
        debug!("Got message to send: {:?}", msg);

        match msg {
            ServerMessage::Close { code, reason } => {
                writer
                    .send(Message::Close(Some(CloseFrame {
                        code,
                        reason: reason.into(),
                    })))
                    .await?;

                writer.close().await?;
                break;
            }

            ServerMessage::Normal(msg) => {
                let msg = BASE64_STANDARD.encode(serde_json::to_string(&msg)?);

                debug!("Encoded data: {msg}");

                writer.send(Message::Text(msg.into())).await?
            }

            ServerMessage::Pong => writer.send(Message::Pong(Bytes::new())).await?,

            ServerMessage::ForceClose => break,
        }
    }

    Ok(())
}
