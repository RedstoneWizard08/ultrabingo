use crate::{levels::MAP_POOLS, pools::Level, types::SteamID};
use rand::random_range;
use std::collections::HashMap;

#[derive(Debug, Clone, PartialEq, PartialOrd, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct GameLevel {
    pub name: String,
    pub id: String,
    #[serde(rename = "claimedBy")]
    pub claimed_by_team: Option<String>,
    pub person_to_beat: Option<SteamID>,
    pub time_to_beat: Option<f64>,
    pub row: usize,
    pub column: usize,
    #[serde(rename = "isAngryLevel")]
    pub is_angry: bool,
    pub angry_parent_bundle: String,
    pub angry_level_id: String,
}

impl GameLevel {
    pub fn new(level: &Level, x: usize, y: usize) -> Self {
        Self {
            name: level.name.into(),
            id: level.scene_name.into(),
            claimed_by_team: None,
            person_to_beat: None,
            time_to_beat: None,
            row: x,
            column: y,
            is_angry: level.is_angry,
            angry_parent_bundle: level.angry_parent_bundle.into(),
            angry_level_id: level.angry_level_id().into(),
        }
    }

    pub fn was_claimed_by(&self, team: impl AsRef<str>) -> bool {
        self.claimed_by_team
            .as_ref()
            .is_some_and(|it| it == team.as_ref())
    }
}

#[derive(Debug, Clone, PartialEq, PartialOrd, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct GameGrid {
    pub size: usize,
    pub level_table: Vec<Vec<GameLevel>>,
    pub reserve_levels: Vec<Level>,
}

#[derive(Debug, Clone, PartialEq, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct ClientGameGrid {
    pub size: usize,
    pub level_table: HashMap<String, GameLevel>,
}

impl From<GameGrid> for ClientGameGrid {
    fn from(value: GameGrid) -> Self {
        Self {
            size: value.size,
            level_table: value
                .level_table
                .iter()
                .enumerate()
                .flat_map(|(x, it)| {
                    it.iter()
                        .enumerate()
                        .map(move |(y, lvl)| (format!("{x}-{y}"), lvl.clone()))
                })
                .collect(),
        }
    }
}

impl GameGrid {
    pub fn new(size: usize, pool_ids: &Vec<String>) -> Self {
        let mut me = Self {
            size,
            level_table: Vec::new(),
            reserve_levels: Vec::new(),
        };

        me.populate_grid(pool_ids);
        me
    }

    pub fn populate_grid(&mut self, pool_ids: &Vec<String>) {
        if pool_ids.is_empty() {
            return;
        }

        let mut pool = Vec::new();

        for id in pool_ids {
            if let Some(data) = MAP_POOLS.get(id) {
                pool.extend_from_slice(data.maps);
            } else {
                warn!("Given map pool ID {id} does not have any data! Skipping...");
                continue;
            }
        }

        self.level_table = Vec::new();

        self.level_table
            .resize_with(self.size, || Vec::with_capacity(self.size));

        for x in 0..self.size {
            for y in 0..self.size {
                let idx = random_range(0..pool.len());
                let level = GameLevel::new(&pool[idx], x, y);

                self.level_table[x][y] = level;
                pool.remove(idx);
            }
        }

        self.reserve_levels = pool;
    }
}
