use diesel::{
    Selectable,
    prelude::{Associations, Identifiable, Insertable, Queryable},
};
use serde::{Deserialize, Serialize};

use crate::{
    enums::{Difficulty, TeamComposition},
    modes::GameModeType,
};

#[derive(
    Debug,
    Clone,
    PartialEq,
    Eq,
    PartialOrd,
    Ord,
    Hash,
    Serialize,
    Deserialize,
    Queryable,
    Selectable,
    Insertable,
    Identifiable,
)]
#[diesel(table_name = crate::schema::rooms)]
pub struct Room {
    pub id: i32,
    pub room_code: String,
    pub hosted_by: i64,
    pub current_players: i32,
    pub has_started: bool,
    pub max_players: i32,
    pub max_teams: i32,
    pub team_composition: TeamComposition,
    pub joinable: bool,
    pub grid_size: i32,
    pub gamemode: GameModeType,
    pub difficulty: Difficulty,
    pub p_rank_required: bool,
    pub disable_campaign_alt_exits: bool,
    pub is_public: bool,
    pub has_ended: bool,
}

#[derive(
    Debug, Clone, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize, Insertable,
)]
#[diesel(table_name = crate::schema::rooms)]
pub struct RoomIn {
    pub room_code: String,
    pub hosted_by: i64,
    pub current_players: i32,
    pub has_started: bool,
    pub max_players: i32,
    pub max_teams: i32,
    pub team_composition: TeamComposition,
    pub joinable: bool,
    pub grid_size: i32,
    pub gamemode: GameModeType,
    pub difficulty: Difficulty,
    pub p_rank_required: bool,
    pub disable_campaign_alt_exits: bool,
    pub is_public: bool,
    pub has_ended: bool,
}

#[derive(
    Debug,
    Clone,
    PartialEq,
    Eq,
    PartialOrd,
    Ord,
    Hash,
    Serialize,
    Deserialize,
    Queryable,
    Selectable,
    Insertable,
    Identifiable,
    Associations,
)]
#[diesel(table_name = crate::schema::active_connections)]
#[diesel(belongs_to(Room))]
pub struct ActiveConnection {
    pub id: i32,
    pub connection_hash: String,
    pub ticket: String,
    pub steam_id: i64,
    pub username: String,
    pub room_id: i32,
    pub is_host: bool,
}

#[derive(
    Debug,
    Clone,
    PartialEq,
    Eq,
    PartialOrd,
    Ord,
    Hash,
    Serialize,
    Deserialize,
    Insertable,
    Associations,
)]
#[diesel(table_name = crate::schema::active_connections)]
#[diesel(belongs_to(Room))]
pub struct ActiveConnectionIn {
    pub connection_hash: String,
    pub ticket: String,
    pub steam_id: i64,
    pub username: String,
    pub room_id: i32,
    pub is_host: bool,
}

#[derive(
    Debug,
    Clone,
    PartialEq,
    Eq,
    PartialOrd,
    Ord,
    Hash,
    Serialize,
    Deserialize,
    Queryable,
    Selectable,
    Insertable,
    Identifiable,
    Associations,
)]
#[diesel(table_name = crate::schema::kicked_players)]
#[diesel(belongs_to(Room))]
pub struct KickedPlayer {
    pub id: i32,
    pub steam_id: i64,
    pub room_id: i32,
}

#[derive(
    Debug,
    Clone,
    PartialEq,
    Eq,
    PartialOrd,
    Ord,
    Hash,
    Serialize,
    Deserialize,
    Insertable,
    Associations,
)]
#[diesel(table_name = crate::schema::kicked_players)]
#[diesel(belongs_to(Room))]
pub struct KickedPlayerIn {
    pub steam_id: i64,
    pub room_id: i32,
}

#[derive(
    Debug,
    Clone,
    PartialEq,
    Eq,
    PartialOrd,
    Ord,
    Hash,
    Serialize,
    Deserialize,
    Queryable,
    Selectable,
    Insertable,
    Identifiable,
)]
#[diesel(table_name = crate::schema::banned_players)]
pub struct BannedPlayer {
    pub id: i32,
    pub steam_id: i64,
    pub ip: Option<String>,
}

#[derive(
    Debug, Clone, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize, Insertable,
)]
#[diesel(table_name = crate::schema::banned_players)]
pub struct BannedPlayerIn {
    pub steam_id: i64,
    pub ip: Option<String>,
}

#[derive(
    Debug,
    Clone,
    PartialEq,
    Eq,
    PartialOrd,
    Ord,
    Hash,
    Serialize,
    Deserialize,
    Queryable,
    Selectable,
    Insertable,
    Identifiable,
)]
#[diesel(table_name = crate::schema::ranks)]
pub struct Rank {
    pub id: i32,
    pub name: String,
}

#[derive(
    Debug, Clone, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize, Insertable,
)]
#[diesel(table_name = crate::schema::ranks)]
pub struct RankIn {
    pub name: String,
}

#[derive(
    Debug,
    Clone,
    PartialEq,
    Eq,
    PartialOrd,
    Ord,
    Hash,
    Serialize,
    Deserialize,
    Queryable,
    Selectable,
    Insertable,
    Identifiable,
    Associations,
)]
#[diesel(table_name = crate::schema::user_ranks)]
#[diesel(belongs_to(Rank))]
pub struct UserRank {
    pub id: i32,
    pub steam_id: i64,
    pub rank_id: i32,
}

#[derive(
    Debug,
    Clone,
    PartialEq,
    Eq,
    PartialOrd,
    Ord,
    Hash,
    Serialize,
    Deserialize,
    Insertable,
    Associations,
)]
#[diesel(table_name = crate::schema::user_ranks)]
#[diesel(belongs_to(Rank))]
pub struct UserRankIn {
    pub steam_id: i64,
    pub rank_id: i32,
}
