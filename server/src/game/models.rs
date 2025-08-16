use diesel::sql_types::SmallInt;
use std::collections::HashMap;

use crate::{
    enums::{Difficulty, EnumError, TeamComposition},
    modes::GameModeType,
    ticketing::RegisterTicket,
    types::SteamID,
};

#[repr(u8)]
#[derive(
    Debug,
    Clone,
    Copy,
    PartialEq,
    Eq,
    PartialOrd,
    Ord,
    Hash,
    Serialize_repr,
    Deserialize_repr,
    Default,
    FromSqlRow,
    AsExpression,
    DbEnum,
)]
#[diesel(sql_type = SmallInt)]
#[diesel_enum(error_fn = EnumError::invalid)]
#[diesel_enum(error_type = EnumError)]
pub enum GameState {
    #[default]
    InLobby = 0,
    InGame = 1,
    GameFinished = 2,
}

#[repr(u8)]
#[derive(
    Debug,
    Clone,
    Copy,
    PartialEq,
    Eq,
    PartialOrd,
    Ord,
    Hash,
    Serialize_repr,
    Deserialize_repr,
    Default,
    FromSqlRow,
    AsExpression,
    DbEnum,
)]
#[diesel(sql_type = SmallInt)]
#[diesel_enum(error_fn = EnumError::invalid)]
#[diesel_enum(error_type = EnumError)]
pub enum GameVisibility {
    #[default]
    Private = 0,
    Public = 1,
}
#[repr(i8)]
#[derive(
    Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize_repr, Deserialize_repr,
)]
pub enum PlayerRemovalResult {
    IsHost = 1,
    Ok = 0,
    PlayerNotFound = -1,
    GameNotFound = -2,
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct GameSettings {
    pub max_players: usize,
    pub max_teams: usize,
    pub time_limit: u32, // minutes
    pub team_composition: TeamComposition,
    pub game_mode: GameModeType,
    pub difficulty: Difficulty,
    pub grid_size: usize,
    pub p_rank_required: bool,
    pub disable_campaign_alt_exits: bool,
    pub visibility: GameVisibility,
    pub domination_timer: u32,
    pub selected_map_pools: Vec<String>,
    pub preset_teams: Option<HashMap<SteamID, String>>,
}

impl Default for GameSettings {
    fn default() -> Self {
        Self {
            max_players: 8,
            max_teams: 4,
            time_limit: 5,
            team_composition: TeamComposition::Random,
            game_mode: GameModeType::Bingo,
            difficulty: Difficulty::Standard,
            grid_size: 3,
            p_rank_required: false,
            disable_campaign_alt_exits: false,
            visibility: GameVisibility::Private,
            domination_timer: 5, // same as time_limit
            selected_map_pools: Vec::new(),
            preset_teams: None,
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct SubmissionData {
    pub team: String,
    pub game_id: i32,
    pub column: usize,
    pub row: usize,
    pub level_name: String,
    pub level_id: String,
    pub player_name: String,
    pub steam_id: String,
    pub time: f32,
    pub ticket: RegisterTicket,
}
