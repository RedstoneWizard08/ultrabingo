use crate::enums::Difficulty;

#[derive(Debug, Clone, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize)]
pub struct PublicGameData {
    #[serde(rename = "R_PASSWORD")]
    pub password: String,

    #[serde(rename = "R_CURRENTPLAYERS")]
    pub current_players: usize,

    #[serde(rename = "R_USERNAME")]
    pub username: String,

    #[serde(rename = "R_MAXPLAYERS")]
    pub max_players: usize,

    #[serde(rename = "R_DIFFICULTY")]
    pub difficulty: Difficulty,
}

#[repr(i8)]
#[derive(
    Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize_repr, Deserialize_repr,
)]
pub enum JoinEligibility {
    Kicked = -6,
    Banned = -5,
    Full = -4,
    NotAccepting = -3,
    AlreadyStarted = -2,
    NotFound = -1,
    Ok = 0,
}
