use anyhow::Result;
use tracing_subscriber::EnvFilter;

#[tokio::main]
pub async fn main() -> Result<()> {
    tracing_subscriber::fmt()
        .with_env_filter(EnvFilter::from_default_env())
        .init();

    bingo_server::server::run_server("0.0.0.0", 4000, None).await
}
