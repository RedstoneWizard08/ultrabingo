use crate::{
    db::DbPool,
    enums::TeamComposition,
    game::{
        Game, GamePlayer,
        models::{GameSettings, GameState, PlayerRemovalResult, SubmissionData},
    },
    messages::{
        EncapsulatedMessage, OutgoingMessage, RoomSettingsUpdate, ServerMessage, SubmissionResult,
    },
    models::{ActiveConnection, ActiveConnectionIn, Room},
    rooms::pool::PooledRoomManager,
    schema::{active_connections, rooms},
    server::HttpState,
    ticketing::RegisterTicket,
    types::SteamID,
    util::sanitize_username,
};
use anyhow::Result;
use chrono::{Local, Utc};
use diesel::{
    BoolExpressionMethods, ExpressionMethods, QueryDsl, RunQueryDsl, SelectableHelper, delete,
    insert_into, update,
};
use parking_lot::RwLock;
use std::{collections::HashMap, sync::Arc};

pub struct GameController {
    pub rooms: Arc<PooledRoomManager>,
    pub current_games: HashMap<i32, Arc<RwLock<Game>>>,
}

impl GameController {
    pub fn new(pool: DbPool) -> Arc<RwLock<Self>> {
        info!(
            "Game controller started at: {}",
            Local::now().format("%Y-%m-%d %H:%M:%S %Z")
        );

        Arc::new(RwLock::new(Self {
            rooms: Arc::new(PooledRoomManager::new(pool)),
            current_games: HashMap::new(),
        }))
    }

    pub fn create_game(&mut self, game_id: i32, host: GamePlayer) -> Result<Arc<RwLock<Game>>> {
        info!(
            "Creating game with ID {game_id}; host = {} ({})",
            host.username, host.steam_id
        );

        let game = Game::new(game_id, Arc::clone(&self.rooms), host)?;

        self.current_games.insert(game_id, Arc::clone(&game));

        Ok(game)
    }

    pub fn join_game(&self, game_id: i32, player: GamePlayer) -> Result<()> {
        if let Some(game) = self.current_games.get(&game_id) {
            game.write().add_player(player.clone(), false)?;

            game.read().broadcast_except(
                player.steam_id,
                OutgoingMessage::JoinRoomNotification {
                    username: player.username,
                    steam_id: format!("{}", player.steam_id),
                    rank: player.rank,
                },
            )
        } else {
            error!(
                "Unable to add player {} ({}) to game {}, as it doesn't exist!",
                player.username, player.steam_id, game_id
            );

            Ok(())
        }
    }

    pub fn kick_player(&self, game_id: i32, player: SteamID) -> Result<()> {
        if let Some(game) = self.current_games.get(&game_id) {
            let mut game = game.upgradable_read();

            if let Some(to_kick) = game.current_players.get(&player) {
                let notif = EncapsulatedMessage::new(OutgoingMessage::KickNotification {
                    player_to_kick: to_kick.username.clone(),
                    steam_id: format!("{}", to_kick.steam_id),
                })?;

                if game.host == Some(to_kick.steam_id) {
                    error!("Host is trying to kick themselves, preventing!");
                    return Ok(());
                }

                for player in game.current_players.values() {
                    if player.steam_id == to_kick.steam_id {
                        player
                            .conn
                            .send(ServerMessage::Normal(EncapsulatedMessage::new(
                                OutgoingMessage::Kicked {},
                            )?))?;
                    } else {
                        player.conn.send(ServerMessage::Normal(notif.clone()))?;
                    }
                }

                if let Ok(mut room) = self.rooms.get_game(game_id) {
                    room.add_kick(&to_kick.steam_id)?;
                } else {
                    warn!(
                        "Unable to persist kick for player {player} in game {game_id}, as the room manager couldn't be found!"
                    );
                }

                game.with_upgraded(|it| it.current_players.remove(&player));
            } else {
                error!(
                    "Unable to kick player {player} from game {game_id}, as they aren't in the game!"
                );
            }
        } else {
            error!("Unable to kick player {player} from game {game_id}, as it doesn't exist!");
        }

        Ok(())
    }

    pub fn update_game_settings(&self, data: RoomSettingsUpdate) -> Result<()> {
        let mut teams_reset = false;

        if let Some(game) = self.current_games.get(&data.room_id) {
            let mut game = game.upgradable_read();

            if let Ok(mut room) = self.rooms.get_game(game.id) {
                let mut settings = GameSettings::default();

                settings.max_players = data.max_players;
                settings.max_teams = data.max_teams;
                settings.team_composition = data.team_composition;
                settings.time_limit = data.time_limit;
                settings.game_mode = data.game_mode;
                settings.difficulty = data.difficulty;
                settings.grid_size = data.grid_size;
                settings.p_rank_required = data.p_rank_required;
                settings.disable_campaign_alt_exits = data.disable_campaign_alt_exits;
                settings.visibility = data.game_visibility;
                settings.selected_map_pools = game.settings.selected_map_pools.clone();
                settings.preset_teams = game.settings.preset_teams.clone();

                if settings.team_composition == TeamComposition::Random
                    && settings.preset_teams.is_some()
                {
                    settings.preset_teams = None;
                    room.set_joinable(true)?;
                    teams_reset = true;
                }

                room.update_settings(&settings)?;

                let msg = OutgoingMessage::RoomUpdate {
                    max_players: settings.max_players,
                    max_teams: settings.max_teams,
                    time_limit: settings.time_limit,
                    team_composition: settings.team_composition,
                    gamemode: settings.game_mode,
                    grid_size: settings.grid_size,
                    difficulty: settings.difficulty,
                    p_rank_required: settings.p_rank_required,
                    disable_campaign_alt_exits: settings.disable_campaign_alt_exits,
                    game_visibility: settings.visibility,
                    were_teams_reset: teams_reset,
                };

                game.with_upgraded(|game| game.settings = settings);

                if let Some(host) = game.host {
                    game.broadcast_except(host, msg)?;
                } else {
                    game.broadcast(msg)?;
                }
            } else {
                error!(
                    "Failed to update settings for game {}, as the room could not be fetched!",
                    game.id
                );
            }
        } else {
            error!(
                "Failed to update settings for game {}, as it doesn't exist!",
                data.room_id
            );
        }

        Ok(())
    }

    pub fn start_game(&self, game_id: i32, state: &HttpState) -> Result<()> {
        if let Some(game) = self.current_games.get(&game_id) {
            if let Ok(mut room) = self.rooms.get_game(game_id) {
                let mut game = game.write();

                if let Some(preset) = game.settings.preset_teams.clone() {
                    game.set_teams_from_preset(preset);
                } else {
                    game.set_teams_random();
                }

                let size = game.settings.grid_size;

                game.state = GameState::InGame;
                game.generate_grid(size);
                game.mode = game.settings.game_mode.mode();

                let mut info = game.info();

                game.mode.setup(&mut info, state)?;
                game.apply(info);

                room.start()?;
                game.start_time = Some(Utc::now());

                info!(
                    "Game {game_id} starting at {}",
                    game.start_time.unwrap().format("%Y-%m-%d %H:%M:%S %Z")
                );

                game.vote_threshold =
                    (game.current_players.len() as f32 * (2.0 / 3.0)).ceil() as usize;

                info!("Minimum votes for map reroll is {}", game.vote_threshold);

                let info = game.info();

                for (id, player) in game.current_players.clone() {
                    game.player_vote_perms.insert(id, true);

                    if let Some(team) = &player.team {
                        player.send(OutgoingMessage::StartGame {
                            game: info.clone(),
                            team_color: team.clone(),
                            teammates: game.teams[team].iter().map(|it| format!("{it}")).collect(),
                            grid: game.grid.clone().into(),
                        })?;
                    } else {
                        error!("No team set for player {id}!");
                    }
                }
            } else {
                error!("Failed to start game {game_id}, as its room couldn't be fetched!");
            }
        } else {
            error!("Failed to start game {game_id}, as it doesn't exist!");
        }

        Ok(())
    }

    pub fn check_player_before_removing(
        &self,
        game_id: i32,
        steam_id: SteamID,
    ) -> PlayerRemovalResult {
        if let Some(game) = self.current_games.get(&game_id) {
            let game = game.read();

            if let Some(player) = game.current_players.get(&steam_id) {
                if Some(player.steam_id) == game.host {
                    info!("Player to remove is the host, deleting the whole game!");
                    PlayerRemovalResult::IsHost
                } else {
                    PlayerRemovalResult::Ok
                }
            } else {
                warn!("Could not find the player to remove in the specified game!");
                PlayerRemovalResult::PlayerNotFound
            }
        } else {
            warn!("Could not find the specified game!");
            PlayerRemovalResult::GameNotFound
        }
    }

    pub fn disconnect_player(
        &self,
        game_id: i32,
        player_name: String,
        steam_id: SteamID,
    ) -> Result<()> {
        if let Some(game) = self.current_games.get(&game_id) {
            if let Ok(mut room) = self.rooms.get_game(game_id) {
                let mut game = game.upgradable_read();

                if game.settings.preset_teams.is_some() {
                    game.with_upgraded(|game| {
                        game.settings.preset_teams = None;
                    });

                    room.set_joinable(true)?;
                }

                let player = &game.current_players[&steam_id];

                game.broadcast_except(
                    steam_id,
                    OutgoingMessage::DisconnectNotification {
                        username: player.username.clone(),
                        steam_id: format!("{}", steam_id),
                    },
                )?;

                self.disconnect(&steam_id)?;
                room.update_player_count(-1)?;

                game.with_upgraded(|game| game.current_players.remove(&steam_id));
            } else {
                error!(
                    "Failed to disconnect player {player_name} ({steam_id}) from game {game_id}, as its room couldn't be fetched!"
                );
            }
        } else {
            error!(
                "Failed to disconnect player {player_name} ({steam_id}) from game {game_id}, as it doesn't exist!"
            );
        }

        Ok(())
    }

    pub fn disconnect_all_players(&self, game_id: i32, reason: Option<String>) -> Result<()> {
        let reason = reason.unwrap_or("UNSPECIFIED".into());

        if let Some(game) = self.current_games.get(&game_id) {
            let mut game = game.write();

            if let Some(host) = game.host {
                game.broadcast_except(
                    host,
                    OutgoingMessage::ServerDisconnection {
                        disconnect_code: 1001,
                        disconnect_message: reason.clone(),
                    },
                )?;

                for player in game.current_players.values() {
                    player.conn.send(ServerMessage::Close {
                        code: 1000,
                        reason: reason.clone(),
                    })?;

                    self.disconnect(&player.steam_id)?;
                }

                game.current_players.clear();
            } else {
                error!(
                    "Failed to disconnect all players from game {game_id} because there was no host!"
                );

                return Ok(());
            }
        } else {
            error!("Failed to disconnect all players from game {game_id}, as it doesn't exist!");
        }

        Ok(())
    }

    pub fn destroy_game(&mut self, game_id: i32) -> Result<()> {
        if let Some(game) = self.current_games.get(&game_id) {
            if let Ok(mut room) = self.rooms.get_game(game_id) {
                info!("Destroying game id {game_id} from game coordinator");

                game.write().ended = true;
                self.current_games.remove(&game_id);

                info!("Clearing kicks");

                room.clear_kicks()?;

                info!("Removing entry from DB");

                room.remove_game()?;
            } else {
                error!("Failed to destroy game {game_id}, since its room couldn't be fetched!");
            }
        } else {
            error!("Failed to destroy game {game_id}, since it doesn't exist!");
        }

        Ok(())
    }

    pub fn verify_run_submission(&self, data: &SubmissionData) -> bool {
        if let Some(game) = self.current_games.get(&data.game_id) {
            let game = game.read();
            let level = &game.grid.level_table[data.row][data.column];

            info!(
                "Player submitting at pos [{}, {}] level ID in server cell is {}",
                data.row, data.column, level.id
            );

            // Check that the level matches

            if data.level_name == level.name {
                true
            } else {
                warn!(
                    "Level ID doesn't match! Got '{}' instead of '{}'",
                    data.level_name, level.name
                );
                false
            }
        } else {
            error!(
                "Cannot verify submission for game {}, as it doesn't exist!",
                data.game_id
            );

            false
        }
    }

    pub fn submit_run(&self, data: SubmissionData) -> Result<SubmissionResult> {
        if let Some(game) = self.current_games.get(&data.game_id) {
            let mut game = game.upgradable_read();
            let level = &game.grid.level_table[data.row][data.column];
            let steam_id = data.steam_id;

            if level.claimed_by_team.is_none() {
                info!(
                    "Level {} is unclaimed, claiming for team: {}",
                    level.name, data.team
                );

                Ok(game.with_upgraded(|game| {
                    let lvl = &mut game.grid.level_table[data.row][data.column];

                    lvl.claimed_by_team = Some(data.team);
                    lvl.person_to_beat = Some(steam_id);
                    lvl.time_to_beat = Some(data.time);

                    game.num_claims += 1;

                    if game.first_map_claimed.is_none() {
                        game.first_map_claimed = Some(lvl.name.clone());
                    }

                    game.last_map_claimed = Some(lvl.name.clone());
                    game.update_best_stat_value(data.time, data.level_name);

                    SubmissionResult::Claimed
                }))
            } else if level.time_to_beat.is_some_and(|it| data.time < it) {
                let improved = level
                    .claimed_by_team
                    .as_ref()
                    .is_some_and(|it| *it == data.team);

                game.with_upgraded(|game| {
                    let lvl = &mut game.grid.level_table[data.row][data.column];

                    lvl.claimed_by_team = Some(data.team);
                    lvl.person_to_beat = Some(steam_id);
                    lvl.time_to_beat = Some(data.time);

                    game.num_claims += 1;

                    if game.first_map_claimed.is_none() {
                        game.first_map_claimed = Some(lvl.name.clone());
                    }

                    game.last_map_claimed = Some(lvl.name.clone());
                    game.update_best_stat_value(data.time, data.level_name);
                });

                if improved {
                    info!("Level already claimed by player/team, improved");
                    Ok(SubmissionResult::Improved)
                } else {
                    info!("Reclaimed level from previous team");
                    Ok(SubmissionResult::Beat)
                }
            } else {
                Ok(SubmissionResult::CriteriaFailed)
            }
        } else {
            error!(
                "Unable to submit run from {} in level {} in game {}, as it doesn't exist!",
                data.player_name, data.level_name, data.game_id
            );

            Ok(SubmissionResult::CriteriaFailed)
        }
    }

    pub fn humiliate_player(&self, game_id: i32, steam_id: SteamID) -> Result<()> {
        if let Some(game) = self.current_games.get(&game_id) {
            let game = game.read();

            if let Some(player) = game.current_players.get(&steam_id) {
                game.broadcast_except(
                    steam_id,
                    OutgoingMessage::CheatNotification {
                        player: player.username.clone(),
                    },
                )?;
            } else {
                error!(
                    "Unable to humiliate player {steam_id} in game {game_id}, since they are not in the game!"
                );
            }
        } else {
            error!("Unable to humiliate player {steam_id} in game {game_id}, as it doesn't exist!");
        }

        Ok(())
    }

    pub fn map_ping(&self, game_id: i32, team: String, row: usize, column: usize) -> Result<()> {
        if let Some(game) = self.current_games.get(&game_id) {
            game.read()
                .broadcast_team(team, OutgoingMessage::MapPing { row, column })?;
        } else {
            error!(
                "Unable to map ping [{row}, {column}] for team {team} in game {game_id}, as it doesn't exist!"
            );
        }

        Ok(())
    }

    pub fn verify_connection(&self, ticket: &RegisterTicket, check_host: bool) -> Result<bool> {
        let res = active_connections::table
            .filter(
                active_connections::steam_id
                    .eq(&ticket.steam_id.parse::<i64>()?)
                    .and(active_connections::room_id.eq(ticket.game_id)),
            )
            .select(ActiveConnection::as_select())
            .get_result(&mut self.rooms.pool.get()?);

        if let Ok(conn) = res {
            let ticket_match = bcrypt::verify(&ticket.steam_ticket, &conn.ticket)?;
            let room_match = ticket.game_id == conn.room_id;
            let host_match = if check_host { conn.is_host } else { true };
            let check = ticket_match && room_match && host_match;

            if check {
                Ok(true)
            } else {
                error!("Connection invalid!");
                error!("-> Steam ticket match: {ticket_match}");
                error!("-> Room match: {room_match}");

                if check_host {
                    warn!("-> Host match: {host_match}");
                }

                Ok(false)
            }
        } else {
            error!("Connection invalid!");
            Ok(false)
        }
    }

    pub fn connect(
        &self,
        conn: impl AsRef<[u8]>,
        steam_ticket: impl AsRef<[u8]>,
        steam_id: &SteamID,
        steam_username: impl AsRef<str>,
        room_id: i32,
    ) -> Result<ActiveConnection> {
        let mut db = self.rooms.pool.get()?;
        let connection_hash = format!("{:x}", md5::compute(conn));
        let ticket = bcrypt::hash(steam_ticket, 12)?;

        let room = rooms::table
            .find(room_id)
            .select(Room::as_select())
            .get_result(&mut db)?;

        let is_host = room.hosted_by == *steam_id;
        let username = sanitize_username(steam_username);

        Ok(insert_into(active_connections::table)
            .values(ActiveConnectionIn {
                connection_hash,
                ticket,
                username,
                is_host,
                steam_id: steam_id.clone(),
                room_id: room.id,
            })
            .returning(ActiveConnection::as_returning())
            .get_result(&mut db)?)
    }

    pub fn update_connection(
        &self,
        steam_id: &SteamID,
        conn: impl AsRef<[u8]>,
        room_id: i32,
    ) -> Result<ActiveConnection> {
        Ok(update(active_connections::table)
            .filter(
                active_connections::steam_id
                    .eq(steam_id)
                    .and(active_connections::room_id.eq(room_id)),
            )
            .set(active_connections::connection_hash.eq(format!("{:x}", md5::compute(conn))))
            .returning(ActiveConnection::as_returning())
            .get_result(&mut self.rooms.pool.get()?)?)
    }

    pub fn disconnect(&self, steam_id: &SteamID) -> Result<ActiveConnection> {
        Ok(delete(active_connections::table)
            .filter(active_connections::steam_id.eq(steam_id))
            .returning(ActiveConnection::as_returning())
            .get_result(&mut self.rooms.pool.get()?)?)
    }
}
