use diesel::{deserialize::FromSqlRow, expression::AsExpression, sql_types::SmallInt};
use diesel_enum::DbEnum;
use thiserror::Error;

#[derive(Debug, Error)]
#[error("Invalid enum value: {msg}")]
pub struct EnumError {
    pub msg: String,
}

impl EnumError {
    pub fn invalid(msg: String) -> Self {
        Self { msg }
    }
}

#[repr(i16)]
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
pub enum TeamComposition {
    Random = 0,
    Manual = 1,
}

#[repr(i16)]
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
pub enum Difficulty {
    Harmless = 0,
    Lenient = 1,
    Standard = 2,
    Violent = 3,
    Brutal = 4,
}
