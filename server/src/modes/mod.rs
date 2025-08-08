use crate::{
    enums::EnumError,
    game::{GameInfo, models::SubmissionData},
    messages::SubmissionResult,
    rooms::manager::RoomManager,
    server::HttpState,
};
use anyhow::Result;
use diesel::sql_types::SmallInt;
use std::fmt::Debug;

pub mod bingo;
pub mod domination;

pub trait GameMode: Send + Sync {
    fn setup(&mut self, game: &mut GameInfo, state: &HttpState) -> Result<()>;

    fn on_map_claim(
        &mut self,
        game: &mut GameInfo,
        room: &mut RoomManager,
        received: SubmissionData,
        submit_result: SubmissionResult,
        map_is_being_voted: bool,
    ) -> Result<()>;

    fn end_game(
        &mut self,
        game: &mut GameInfo,
        room: &mut RoomManager,
        received: SubmissionData,
    ) -> Result<()>;
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
    FromSqlRow,
    AsExpression,
    DbEnum,
)]
#[diesel(sql_type = SmallInt)]
#[diesel_enum(error_fn = EnumError::invalid)]
#[diesel_enum(error_type = EnumError)]
pub enum GameModeType {
    Bingo = 0,
    Domination = 1,
}

impl GameModeType {
    pub fn mode(&self) -> Box<dyn GameMode> {
        match *self {
            GameModeType::Bingo => Box::new(bingo::BingoGamemode),
            GameModeType::Domination => Box::new(domination::DominationGamemode),
        }
    }
}
