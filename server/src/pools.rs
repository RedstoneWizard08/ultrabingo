#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize)]
pub struct Level {
    pub scene_name: &'static str,
    pub name: &'static str,
    pub is_angry: bool,
    pub angry_parent_bundle: &'static str,
}

impl Level {
    pub fn angry_level_id(&self) -> &'static str {
        if self.is_angry { self.scene_name } else { "" }
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize)]
pub struct MapPool {
    pub name: &'static str,
    pub description: &'static str,
    pub maps: &'static [Level],
}
