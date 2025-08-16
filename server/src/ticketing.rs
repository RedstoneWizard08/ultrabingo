#[derive(Debug, Clone, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct RegisterTicket {
    pub steam_ticket: String,
    pub steam_id: String,
    pub steam_username: String,
    pub game_id: i32,
}
