use anyhow::Result;
use std::collections::HashMap;
use strum::AsRefStr;

use crate::{
    enums::{Difficulty, TeamComposition},
    game::{
        grid::{ClientGameGrid, GameLevel}, models::{GameVisibility, SubmissionData}, GameInfo
    },
    modes::GameModeType,
    rooms::models::{JoinEligibility, PublicGameData},
    ticketing::RegisterTicket,
};

#[repr(i8)]
#[derive(
    Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize_repr, Deserialize_repr,
)]
pub enum SubmissionResult {
    /// Submission did not beat the criteria.
    CriteriaFailed = -1,

    /// Submission claimed an unclaimed map.
    Claimed = 0,

    /// Submission improved an already claimed map.
    Improved = 1,

    /// Submission beat criteria.
    Beat = 2,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize)]
#[serde(rename_all = "lowercase")]
pub enum RoomCreateStatus {
    Ban,
    Ok,
    Err,
}

#[repr(u8)]
#[derive(
    Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize_repr, Deserialize_repr,
)]
pub enum GameEndStatus {
    Win = 0,
    NoClaims = 1,
    Tie = 2,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize)]
#[serde(rename_all = "lowercase")]
pub enum FetchGamesStatus {
    Ok,
    None,
}

#[derive(
    Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize, Default,
)]
#[serde(rename_all = "lowercase")]
pub enum PongStatus {
    #[default]
    Ok,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize)]
#[serde(rename_all = "UPPERCASE")]
pub enum ReconnectStatus {
    Ok,
    End,
}

#[repr(u8)]
#[derive(
    Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize_repr, Deserialize_repr,
)]
pub enum RerollNotifType {
    Start = 0,
    Vote = 1,
}

#[repr(u8)]
#[derive(
    Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize_repr, Deserialize_repr,
)]
pub enum UpdateTeamsStatus {
    Set = 0,
    Clear = 1,
}

#[derive(Debug, Serialize, AsRefStr)]
#[serde(untagged)]
#[serde(rename_all_fields = "PascalCase")]
pub enum OutgoingMessage {
    CheatNotification {
        #[serde(rename = "PlayerToHumiliate")]
        player: String,
    },

    CreateRoomResponse {
        status: RoomCreateStatus,
        room_id: i32,
        room_details: Option<GameInfo>,
        room_password: String,
    },

    DisconnectNotification {
        username: String,
        steam_id: String,
    },

    FetchGamesResponse {
        status: FetchGamesStatus,
        game_data: Vec<PublicGameData>,
    },

    GameEnd {
        winning_team: String,
        winning_players: Vec<String>,
        time_elapsed: String,
        claims: usize,
        first_map_claimed: Option<String>,
        last_map_claimed: Option<String>,
        best_stat_value: Option<f32>,
        best_stat_map: Option<String>,
        end_status: GameEndStatus,
        tied_teams: Vec<String>,
    },

    HostMigration {
        old_host: String,
        host_username: String,
        host_steam_id: String,
    },

    JoinRoomNotification {
        username: String,
        steam_id: String,
        rank: String,
    },

    JoinRoomResponse {
        status: JoinEligibility,
        room_id: i32,
        room_details: Option<GameInfo>,
    },

    Kicked {},

    KickNotification {
        player_to_kick: String,
        steam_id: String,
    },

    LevelClaimed {
        claim_type: SubmissionResult,
        username: String,
        level_name: String,
        team: String,
        row: usize,
        column: usize,
        new_time_requirement: f32,
        is_map_voted: bool,
    },

    MapPing {
        row: usize,
        column: usize,
    },

    ModVerificationResponse {
        non_whitelisted_mods: Vec<String>,
        /// See [`crate::util::CLIENT_VERSION`]
        latest_version: String,
        motd: String,
        available_ranks: String,
    },

    ReconnectResponse {
        status: ReconnectStatus,
        game_data: Option<GameInfo>,
    },

    RerollExpire {
        map_name: String,
    },

    RerollSuccess {
        old_map_id: String,
        old_map_name: String,
        map_data: GameLevel,
        location_x: usize,
        location_y: usize,
    },

    RerollVote {
        map_name: String,
        vote_starter: String,
        vote_starter_steam_id: String,
        num_votes: usize,
        votes_required: usize,
        notif_type: RerollNotifType,

        /// This is just the length of time for the reroll to take place.
        /// See [`crate::util::VOTE_TIMER_SECS`].
        timer: u64,
    },

    RoomUpdate {
        max_players: usize,
        max_teams: usize,
        time_limit: u32,
        team_composition: TeamComposition,
        gamemode: GameModeType,
        p_rank_required: bool,
        difficulty: Difficulty,
        grid_size: usize,
        disable_campaign_alt_exits: bool,
        game_visibility: GameVisibility,
        were_teams_reset: bool,
    },

    ServerDisconnection {
        disconnect_code: u32,
        disconnect_message: String,
    },

    StartGame {
        game: GameInfo,
        team_color: String,
        teammates: Vec<String>,
        grid: ClientGameGrid,
    },

    Timeout {
        player: String,
        steam_id: String,
    },

    UpdateTeams {
        status: UpdateTeamsStatus,
    },

    Pong {
        status: PongStatus,
    },
}

#[derive(Debug, Clone, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct EncapsulatedMessage {
    pub message_type: String,
    pub contents: String,
}

impl EncapsulatedMessage {
    pub fn new(msg: OutgoingMessage) -> Result<Self> {
        let name: &str = msg.as_ref();

        Ok(Self {
            message_type: name.into(),
            contents: serde_json::to_string(&msg)?,
        })
    }
}

#[derive(Debug)]
pub enum ServerMessage {
    Normal(EncapsulatedMessage),
    Pong,
    ForceClose,

    Close { code: u16, reason: String },
}

#[derive(Debug, Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct RoomSettingsUpdate {
    pub room_id: i32,
    pub max_players: usize,
    pub max_teams: usize,
    pub time_limit: u32,
    pub team_composition: TeamComposition,
    pub game_mode: GameModeType,
    pub p_rank_required: bool,
    pub difficulty: Difficulty,
    pub grid_size: usize,
    pub disable_campaign_alt_exits: bool,
    pub game_visibility: GameVisibility,
    pub ticket: RegisterTicket,
}

#[derive(Debug, Serialize, Deserialize, AsRefStr)]
#[serde(tag = "MessageType", content = "Contents")]
#[serde(rename_all_fields = "PascalCase")]
pub enum IncomingMessage {
    CheatActivation {
        game_id: i32,
        username: String,
        steam_id: String,
    },

    ClearTeams {
        game_id: i32,
        ticket: RegisterTicket,
    },

    CreateRoom {
        room_name: String,
        room_password: String,
        max_players: usize,
        p_rank_required: bool,
        host_steam_name: String,
        host_steam_id: String,
        rank: String,
    },

    FetchGames {},

    JoinRoom {
        password: String,
        username: String,
        steam_id: String,
        rank: String,
    },

    KickPlayer {
        game_id: i32,
        player_to_kick: String,
        ticket: RegisterTicket,
    },

    LeaveGame {
        room_id: i32,
        username: Option<String>,
        steam_id: String,
    },

    MapPing {
        game_id: i32,
        team: String,
        row: usize,
        column: usize,
        ticket: RegisterTicket,
    },

    ReconnectRequest {
        room_id: i32,
        steam_id: String,
        ticket: RegisterTicket,
    },

    RegisterTicket(RegisterTicket),

    RerollRequest {
        steam_ticket: RegisterTicket,
        steam_id: String,
        game_id: i32,
        row: usize,
        column: usize,
    },

    StartGame {
        room_id: i32,
        ticket: RegisterTicket,
    },

    SubmitRun(SubmissionData),

    UpdateTeamSettings {
        game_id: i32,
        teams: HashMap<String, usize>,
        ticket: RegisterTicket,
    },

    UpdateMapPool {
        game_id: i32,
        map_pool_ids: Vec<String>,
        ticket: RegisterTicket,
    },

    UpdateRoomSettings(RoomSettingsUpdate),

    VerifyModList {
        client_mod_list: Vec<String>,
        steam_id: String,
    },
}
