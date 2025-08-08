use crate::{
    db::{establish_connection, persist_tables_env, run_migrations},
    game::controller::GameController,
    logger::logging_middleware,
};
use anyhow::Result;
use axum::{
    Router,
    middleware::from_fn,
    routing::{any, get},
    serve,
};
use parking_lot::RwLock;
use std::{net::SocketAddr, sync::Arc};
use tokio::net::TcpListener;

pub mod pools;
pub mod sockets;

pub type HttpState = Arc<RwLock<GameController>>;

pub async fn run_server(
    host: impl AsRef<str>,
    port: u32,
    persist_tables: Option<bool>,
) -> Result<()> {
    info!("Connecting to the database...");

    let pool = establish_connection()?;

    info!("Running migrations...");

    run_migrations(&pool, persist_tables.unwrap_or_else(persist_tables_env))?;

    info!("Creating state...");

    let controller = GameController::new(pool);

    info!("Building router...");

    let router = Router::new()
        .route("/maps", get(pools::pool_handler))
        .route("/bingoMapPool.toml", get(pools::pool_handler))
        .route("/", any(sockets::handle_upgrade))
        .layer(from_fn(logging_middleware))
        .with_state(controller)
        .into_make_service_with_connect_info::<SocketAddr>();

    info!("Binding listener...");

    let listener = TcpListener::bind(format!("{}:{}", host.as_ref(), port)).await?;

    info!("Starting server on {}:{}...", host.as_ref(), port);

    serve(listener, router).await?;

    Ok(())
}
