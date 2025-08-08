pub mod controller;
pub mod grid;
pub mod models;

use anyhow::Result;
use chrono::{DateTime, Utc};
use crossbeam::channel::Sender;
use derivative::Derivative;
use itertools::Itertools;
use lazy_static::lazy_static;
use parking_lot::{Mutex, RwLock};
use rand::seq::SliceRandom;
use serde_with::{DisplayFromStr, serde_as};
use tokio::task::AbortHandle;

use crate::{
    game::{
        grid::{ClientGameGrid, GameGrid, GameLevel},
        models::{GameSettings, GameState},
    },
    messages::{
        EncapsulatedMessage, OutgoingMessage, RerollNotifType, ServerMessage, UpdateTeamsStatus,
    },
    modes::{GameMode, GameModeType},
    rooms::pool::PooledRoomManager,
    server::HttpState,
    types::SteamID,
    util::{ONE_SECOND, VOTE_TIMER_SECS, random_element_pop},
};
use std::{collections::HashMap, sync::Arc, time::SystemTime};

lazy_static! {
    static ref VOTE_TIMERS: Arc<Mutex<HashMap<i32, AbortHandle>>> =
        Arc::new(Mutex::new(HashMap::new()));
}

#[serde_as]
#[derive(Debug, Clone, Derivative, Serialize)]
#[derivative(PartialEq, PartialOrd, Hash)]
#[serde(rename_all = "camelCase")]
pub struct GamePlayer {
    #[serde_as(as = "DisplayFromStr")]
    pub steam_id: SteamID,
    pub username: String,
    pub rank: String,
    pub team: Option<String>,

    #[serde(skip)]
    #[derivative(PartialEq = "ignore")]
    #[derivative(PartialOrd = "ignore")]
    #[derivative(Hash = "ignore")]
    pub conn: Sender<ServerMessage>,
}

impl GamePlayer {
    pub fn send(&self, msg: OutgoingMessage) -> Result<()> {
        self.conn
            .send(ServerMessage::Normal(EncapsulatedMessage::new(msg)?))?;

        Ok(())
    }
}

#[derive(Derivative, Serialize)]
#[derivative(Debug)]
#[serde(rename_all = "camelCase")]
pub struct Game {
    pub id: i32,
    pub grid: GameGrid,
    pub host: Option<i64>,
    pub state: GameState,
    // NOTE: In the PHP code this was an array, here it's a map for faster lookups
    pub current_players: HashMap<SteamID, GamePlayer>,
    pub teams: HashMap<String, Vec<SteamID>>,
    pub settings: GameSettings,
    #[serde(skip)]
    #[derivative(Debug = "ignore")]
    pub mode: Box<dyn GameMode>,
    pub ended: bool,
    pub first_map_claimed: Option<String>,
    pub last_map_claimed: Option<String>,
    pub num_claims: usize,
    pub start_time: Option<DateTime<Utc>>,
    pub end_time: Option<DateTime<Utc>>,
    pub best_stat_map: Option<String>,
    pub best_stat_value: Option<f64>,
    pub is_vote_active: bool,
    pub current_votes: usize,
    pub vote_pos: Option<[usize; 2]>,
    pub vote_threshold: usize,
    pub player_vote_perms: HashMap<SteamID, bool>,
    pub players_already_voted: Vec<SteamID>,
    pub team_names: Vec<String>,

    #[serde(skip)]
    #[derivative(Debug = "ignore")]
    rooms: Arc<PooledRoomManager>,
}

#[derive(Debug, Clone, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct GameInfo {
    pub id: i32,
    #[serde(skip)]
    pub grid: GameGrid,
    #[serde(rename = "grid")]
    pub client_grid: ClientGameGrid,
    pub host: Option<i64>,
    pub state: GameState,
    pub current_players: HashMap<SteamID, GamePlayer>,
    pub teams: HashMap<String, Vec<SteamID>>,
    pub settings: GameSettings,
    pub ended: bool,
    pub first_map_claimed: Option<String>,
    pub last_map_claimed: Option<String>,
    pub num_claims: usize,
    pub start_time: Option<DateTime<Utc>>,
    pub end_time: Option<DateTime<Utc>>,
    pub best_stat_map: Option<String>,
    pub best_stat_value: Option<f64>,
    pub is_vote_active: bool,
    pub current_votes: usize,
    pub vote_pos: Option<[usize; 2]>,
    pub vote_threshold: usize,
    pub player_vote_perms: HashMap<SteamID, bool>,
    pub players_already_voted: Vec<SteamID>,
    pub team_names: Vec<String>,
}

impl Game {
    pub fn new(
        id: i32,
        rooms: Arc<PooledRoomManager>,
        host: GamePlayer,
    ) -> Result<Arc<RwLock<Self>>> {
        let settings = GameSettings::default();

        let mut me = Self {
            id,
            current_players: HashMap::new(),
            ended: false,
            host: None,
            grid: GameGrid::new(settings.grid_size, &settings.selected_map_pools),
            settings,
            state: GameState::InLobby,
            num_claims: 0,
            first_map_claimed: None,
            last_map_claimed: None,
            best_stat_value: None,
            best_stat_map: None,
            is_vote_active: false,
            current_votes: 0,
            vote_threshold: 0,
            vote_pos: None,
            player_vote_perms: HashMap::new(),
            players_already_voted: Vec::new(),
            start_time: None,
            end_time: None,
            teams: HashMap::new(),
            mode: GameModeType::Bingo.mode(),
            rooms,
            team_names: vec!["Red".into(), "Blue".into(), "Green".into(), "Yellow".into()],
        };

        me.add_player(host, true)?;

        Ok(Arc::new(RwLock::new(me)))
    }

    pub fn add_player(&mut self, player: GamePlayer, is_host: bool) -> Result<()> {
        info!(
            "Adding {} ({}) to game {}",
            player.username, player.steam_id, self.id
        );

        if is_host {
            self.host = Some(player.steam_id);
        }

        self.current_players.insert(player.steam_id, player);

        if let Ok(mut room) = self.rooms.get_game(self.id) {
            room.update_player_count(1)
        } else {
            error!("Could not find room with ID {}!", self.id);
            Ok(())
        }
    }

    pub fn apply(&mut self, info: GameInfo) {
        self.id = info.id;
        self.grid = info.grid;
        self.host = info.host;
        self.state = info.state;
        self.current_players = info.current_players;
        self.teams = info.teams;
        self.settings = info.settings;
        self.ended = info.ended;
        self.first_map_claimed = info.first_map_claimed;
        self.last_map_claimed = info.last_map_claimed;
        self.num_claims = info.num_claims;
        self.start_time = info.start_time;
        self.end_time = info.end_time;
        self.best_stat_map = info.best_stat_map;
        self.best_stat_value = info.best_stat_value;
        self.is_vote_active = info.is_vote_active;
        self.current_votes = info.current_votes;
        self.vote_pos = info.vote_pos;
        self.vote_threshold = info.vote_threshold;
        self.player_vote_perms = info.player_vote_perms;
        self.players_already_voted = info.players_already_voted;
        self.team_names = info.team_names;
    }

    pub fn info(&self) -> GameInfo {
        GameInfo {
            id: self.id,
            grid: self.grid.clone(),
            host: self.host,
            state: self.state,
            current_players: self.current_players.clone(),
            teams: self.teams.clone(),
            settings: self.settings.clone(),
            ended: self.ended,
            first_map_claimed: self.first_map_claimed.clone(),
            last_map_claimed: self.last_map_claimed.clone(),
            num_claims: self.num_claims,
            start_time: self.start_time,
            end_time: self.end_time,
            best_stat_map: self.best_stat_map.clone(),
            best_stat_value: self.best_stat_value,
            is_vote_active: self.is_vote_active,
            current_votes: self.current_votes,
            vote_pos: self.vote_pos,
            vote_threshold: self.vote_threshold,
            player_vote_perms: self.player_vote_perms.clone(),
            players_already_voted: self.players_already_voted.clone(),
            team_names: self.team_names.clone(),
            client_grid: self.grid.clone().into(),
        }
    }

    pub fn cancel_reroll_vote(&mut self) {
        self.is_vote_active = false;
        self.current_votes = 0;
    }

    pub fn has_player_voted(&self, steam_id: SteamID) -> bool {
        self.players_already_voted.contains(&steam_id)
    }

    pub fn reset_vote_variables(&mut self) {
        self.is_vote_active = false;
        self.current_votes = 0;
        self.vote_pos = None;
        self.players_already_voted.clear();
    }

    pub fn reroll_map(&mut self, [x, y]: [usize; 2]) -> Result<()> {
        let old_map = self.grid.level_table[x][y].clone();
        let new_map = random_element_pop(&mut self.grid.reserve_levels);
        let new_level = GameLevel::new(&new_map, x, y);

        self.grid.level_table[x][y] = new_level.clone();

        info!("New rolled map is: {}", &new_level.name);

        self.broadcast(OutgoingMessage::RerollSuccess {
            old_map_id: old_map.id,
            old_map_name: old_map.name,
            map_data: new_level,
            location_x: x,
            location_y: y,
        })
    }

    pub fn handle_vote_end(&mut self) -> Result<()> {
        if self.ended {
            return Ok(());
        }

        info!(
            "Received {}, min required was {}",
            self.current_votes, self.vote_threshold
        );

        if let Some([x, y]) = self.vote_pos {
            if self.current_votes >= self.vote_threshold {
                self.reroll_map([x, y])?;
            } else {
                self.broadcast(OutgoingMessage::RerollExpire {
                    map_name: self.grid.level_table[x][y].name.clone(),
                })?;
            }
        } else {
            error!("Failed to re-roll map: no reroll position was set!");
        }

        self.reset_vote_variables();
        Ok(())
    }

    pub fn add_player_vote(&mut self, steam_id: SteamID) -> Result<()> {
        if let Some([x, y]) = self.vote_pos {
            self.current_votes += 1;
            self.players_already_voted.push(steam_id);

            self.broadcast(OutgoingMessage::RerollVote {
                map_name: self.grid.level_table[x][y].name.clone(),
                vote_starter: self.current_players[&steam_id].username.clone(),
                vote_starter_steam_id: format!("{}", steam_id),
                num_votes: self.current_votes,
                votes_required: self.vote_threshold,
                notif_type: RerollNotifType::Vote,
                timer: VOTE_TIMER_SECS.as_secs(),
            })
        } else {
            error!("Failed to add re-roll vote for user {steam_id}: no vote position was set!");
            Ok(())
        }
    }

    pub fn can_player_start_vote(&self, steam_id: SteamID) -> bool {
        self.player_vote_perms
            .get(&steam_id)
            .copied()
            .unwrap_or(false)
    }

    /// IMPORTANT NOTE: In other functions, this will have y before x. DO NOT REVERSE THE ORDER, FIX IT WHERE IT GETS USED!
    pub fn start_reroll_vote(
        &mut self,
        state: &HttpState,
        steam_id: SteamID,
        x: usize,
        y: usize,
    ) -> Result<()> {
        let game_id = self.id;
        let start = SystemTime::now();
        let state = Arc::clone(&state);

        VOTE_TIMERS.lock().insert(
            self.id,
            tokio::task::spawn(async move {
                while start.elapsed().is_ok_and(|it| it < VOTE_TIMER_SECS) {
                    tokio::time::sleep(ONE_SECOND).await;
                }

                let state = state.read();

                if let Some(game) = state.current_games.get(&game_id) {
                    let mut game = game.upgradable_read();

                    if game.is_vote_active {
                        game.with_upgraded(|it| it.handle_vote_end().unwrap());
                    }
                } else {
                    error!("Could not cancel vote for game {game_id}, it doesn't exist!");
                }
            })
            .abort_handle(),
        );

        self.is_vote_active = true;
        self.player_vote_perms.clear();
        self.player_vote_perms.insert(steam_id, false);
        self.vote_pos = Some([x, y]);
        self.add_player_vote(steam_id)?;

        self.broadcast(OutgoingMessage::RerollVote {
            map_name: self.grid.level_table[x][y].name.clone(),
            vote_starter: self.current_players[&steam_id].username.clone(),
            vote_starter_steam_id: format!("{}", steam_id),
            num_votes: self.current_votes,
            votes_required: self.vote_threshold,
            notif_type: RerollNotifType::Start,
            timer: VOTE_TIMER_SECS.as_secs(),
        })
    }

    pub fn remove_player_from_game(&mut self, steam_id: SteamID) {
        self.current_players.remove(&steam_id);
    }

    pub fn put_player_in_team(&mut self, player: SteamID, team: impl AsRef<str>) {
        let team = team.as_ref();

        if !self.teams.contains_key(team) {
            self.teams.insert(team.to_string(), Vec::new());
        }

        self.teams.get_mut(team).unwrap().push(player);
    }

    pub fn update_map_pool(&mut self, pools: Vec<String>) {
        self.settings.selected_map_pools = pools;
    }

    pub fn update_teams(&mut self, dict: HashMap<SteamID, String>) -> Result<()> {
        info!(
            "Manually setting teams for room {} and looking room",
            self.id
        );

        self.settings.preset_teams = Some(dict);

        if let Ok(mut room) = self.rooms.get_game(self.id) {
            room.set_joinable(false)?;
        } else {
            error!(
                "Failed to complete teams update: Could not find room with ID {}!",
                self.id
            );
        }

        self.broadcast(OutgoingMessage::UpdateTeams {
            status: UpdateTeamsStatus::Set,
        })
    }

    pub fn clear_teams(&mut self) -> Result<()> {
        info!("Clearing set teams for room {} and unlocking room", self.id);

        self.settings.preset_teams = None;

        if let Ok(mut room) = self.rooms.get_game(self.id) {
            room.set_joinable(true)?;
        } else {
            error!(
                "Failed to complete clearing teams: Could not find room with ID {}!",
                self.id
            );
        }

        self.broadcast(OutgoingMessage::UpdateTeams {
            status: UpdateTeamsStatus::Clear,
        })
    }

    pub fn update_best_stat_value(&mut self, value: f64, map: String) {
        // Shouldn't this be `value > it`? Idk...
        if self.best_stat_value.is_none_or(|it| value < it) {
            self.best_stat_value = Some(value);
            self.best_stat_map = Some(map);
        }
    }

    pub fn check_bingo(&self, team: impl AsRef<str>, x: usize, y: usize) -> bool {
        // Horizontal
        let horiz = !self
            .grid
            .level_table
            .iter()
            .any(|it| !it[y].was_claimed_by(&team));

        // Vertical
        let vert = !self.grid.level_table[x]
            .iter()
            .any(|level| !level.was_claimed_by(&team));

        // Diag down
        let diag_down = !self
            .grid
            .level_table
            .iter()
            .enumerate()
            .any(|(idx, it)| !it[idx].was_claimed_by(&team));

        // Diag up
        let diag_up = !self
            .grid
            .level_table
            .iter()
            .rev()
            .zip(0..self.grid.size)
            .any(|(it, helper)| !it[helper].was_claimed_by(&team));

        horiz || vert || diag_down || diag_up
    }

    pub fn set_teams_from_preset(&mut self, preset: HashMap<SteamID, String>) {
        self.team_names = preset.values().cloned().unique().collect();

        for (steam_id, team) in preset {
            if let Some(player) = self.current_players.get_mut(&steam_id) {
                player.team = Some(team.clone());
                self.put_player_in_team(steam_id, team);
            } else {
                warn!("Unable to find player {steam_id}, cannot place in team {team}!");
            }
        }

        info!("Set preset teams for game {}:", self.id);
        self.print_teams();
    }

    pub fn print_teams(&self) {
        for (team, players) in &self.teams {
            info!(
                "-> {team}: {}",
                players
                    .iter()
                    .map(|it| format!("{}", it))
                    .collect::<Vec<_>>()
                    .join(", ")
            );
        }
    }

    pub fn set_teams_random(&mut self) {
        let mut entries = self.current_players.keys().copied().collect_vec();

        entries.shuffle(&mut rand::rng());

        let mut team = 0;

        for id in entries {
            if let Some(player) = self.current_players.get_mut(&id) {
                let team_name = self.team_names[team].clone();

                player.team = Some(team_name.clone());
                self.put_player_in_team(id, team_name);

                team = if team == (self.team_names.len() - 1) {
                    0
                } else {
                    team
                };
            } else {
                warn!("Unable to find player {id}, cannot place in team!");
            }
        }

        info!("Set teams for game {}:", self.id);
        self.print_teams();
    }

    pub fn generate_grid(&mut self, size: usize) {
        self.settings.grid_size = size;
        self.grid = GameGrid::new(size, &self.settings.selected_map_pools);
    }

    pub fn broadcast(&self, msg: OutgoingMessage) -> Result<()> {
        let msg = EncapsulatedMessage::new(msg)?;

        for player in self.current_players.values() {
            player.conn.send(ServerMessage::Normal(msg.clone()))?;
        }

        Ok(())
    }

    pub fn broadcast_except(&self, steam_id: SteamID, msg: OutgoingMessage) -> Result<()> {
        let msg = EncapsulatedMessage::new(msg)?;

        for player in self.current_players.values() {
            if player.steam_id == steam_id {
                continue;
            }

            player.conn.send(ServerMessage::Normal(msg.clone()))?;
        }

        Ok(())
    }

    pub fn broadcast_team(&self, team: impl AsRef<str>, msg: OutgoingMessage) -> Result<()> {
        let msg = EncapsulatedMessage::new(msg)?;

        for player in self.current_players.values() {
            if !player.team.as_ref().is_some_and(|it| *it == team.as_ref()) {
                continue;
            }

            player.conn.send(ServerMessage::Normal(msg.clone()))?;
        }

        Ok(())
    }
}

impl GameInfo {
    pub fn check_bingo(&self, team: impl AsRef<str>, x: usize, y: usize) -> bool {
        // Horizontal
        let horiz = !self
            .grid
            .level_table
            .iter()
            .any(|it| !it[y].was_claimed_by(&team));

        // Vertical
        let vert = !self.grid.level_table[x]
            .iter()
            .any(|level| !level.was_claimed_by(&team));

        // Diag down
        let diag_down = !self
            .grid
            .level_table
            .iter()
            .enumerate()
            .any(|(idx, it)| !it[idx].was_claimed_by(&team));

        // Diag up
        let diag_up = !self
            .grid
            .level_table
            .iter()
            .rev()
            .zip(0..self.grid.size)
            .any(|(it, helper)| !it[helper].was_claimed_by(&team));

        horiz || vert || diag_down || diag_up
    }

    pub fn broadcast(&self, msg: OutgoingMessage) -> Result<()> {
        let msg = EncapsulatedMessage::new(msg)?;

        for player in self.current_players.values() {
            player.conn.send(ServerMessage::Normal(msg.clone()))?;
        }

        Ok(())
    }

    pub fn broadcast_except(&self, steam_id: SteamID, msg: OutgoingMessage) -> Result<()> {
        let msg = EncapsulatedMessage::new(msg)?;

        for player in self.current_players.values() {
            if player.steam_id == steam_id {
                continue;
            }

            player.conn.send(ServerMessage::Normal(msg.clone()))?;
        }

        Ok(())
    }

    pub fn broadcast_team(&self, team: impl AsRef<str>, msg: OutgoingMessage) -> Result<()> {
        let msg = EncapsulatedMessage::new(msg)?;

        for player in self.current_players.values() {
            if !player.team.as_ref().is_some_and(|it| *it == team.as_ref()) {
                continue;
            }

            player.conn.send(ServerMessage::Normal(msg.clone()))?;
        }

        Ok(())
    }
}
