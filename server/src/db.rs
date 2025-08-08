use crate::schema::{active_connections, rooms};
use anyhow::{Result, anyhow};
use diesel::{
    RunQueryDsl, SqliteConnection, delete,
    r2d2::{ConnectionManager, Pool, PooledConnection},
    sqlite::Sqlite,
};
use diesel_migrations::{EmbeddedMigrations, MigrationHarness, embed_migrations};
use dotenvy::dotenv;
use std::env;

pub const MIGRATIONS: EmbeddedMigrations = embed_migrations!("migrations");

pub type Db = Sqlite;
pub type DbConn = SqliteConnection;
pub type DbPool = Pool<ConnectionManager<DbConn>>;
pub type PooledConn = PooledConnection<ConnectionManager<DbConn>>;

pub fn persist_tables_env() -> bool {
    if let Some(value) = env::var("PERSIST_TABLES").ok() {
        value == "1"
    } else {
        false
    }
}

pub fn establish_connection() -> Result<DbPool> {
    dotenv().ok();

    Ok(Pool::builder()
        .test_on_check_out(true)
        .build(ConnectionManager::<DbConn>::new(env::var("DATABASE_URL")?))?)
}

pub fn run_migrations(pool: &DbPool, persist: bool) -> Result<()> {
    let mut conn = pool.get()?;

    conn.run_pending_migrations(MIGRATIONS)
        .map_err(|err| anyhow!("{}", err))?;

    if !persist {
        delete(rooms::table).execute(&mut conn)?;
        delete(active_connections::table).execute(&mut conn)?;
    }

    Ok(())
}
