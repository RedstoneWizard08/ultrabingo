use crate::levels::MAP_POOLS;
use axum::{
    http::header::CONTENT_TYPE,
    response::{Response, Result},
};
use indexmap::IndexMap;
use once_cell::sync::Lazy;

pub const POOL_INDEX_VERSION: u8 = 1;
pub const POOL_INDEX: Lazy<MapPoolIndex> = Lazy::new(MapPoolIndex::new);

#[derive(Debug, Clone, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize)]
pub struct MapPoolInfo {
    pub name: String,
    pub description: String,
    #[serde(rename = "numOfMaps")]
    pub num_maps: usize,
    pub maps: Vec<String>,
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct MapPoolIndex {
    pub version: u8,

    // use an IndexMap to preserve the order
    #[serde(rename = "mapPools")]
    pub pools: IndexMap<String, MapPoolInfo>,
}

impl MapPoolIndex {
    fn new() -> Self {
        Self {
            version: POOL_INDEX_VERSION,
            pools: MAP_POOLS
                .entries()
                .map(|(k, pool)| {
                    (
                        k.to_string(),
                        MapPoolInfo {
                            name: pool.name.into(),
                            description: pool.description.replace("\n", "\\n").into(),
                            num_maps: pool.maps.len(),
                            maps: pool.maps.iter().map(|map| map.name.into()).collect(),
                        },
                    )
                })
                .collect(),
        }
    }
}

#[axum::debug_handler]
pub async fn pool_handler() -> Result<Response> {
    Ok(Response::builder()
        .status(200)
        .header(CONTENT_TYPE, "text/plain")
        .body(
            toml::to_string(&*POOL_INDEX)
                .map_err(|it| format!("{it}"))?
                .into(),
        )
        .unwrap())
}
