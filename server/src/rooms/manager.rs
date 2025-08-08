use crate::{
    db::PooledConn,
    game::models::{GameSettings, GameVisibility},
    models::{ActiveConnection, KickedPlayer, KickedPlayerIn, Room},
    rooms::models::JoinEligibility,
    schema::{active_connections, banned_players, kicked_players, rooms},
    types::SteamID,
};
use anyhow::Result;
use diesel::{
    BoolExpressionMethods, ExpressionMethods, QueryDsl, RunQueryDsl, SelectableHelper, delete,
    insert_into, update,
};

pub struct RoomManager {
    conn: PooledConn,
    pub room: Room,
}

impl RoomManager {
    pub fn new(conn: PooledConn, room: Room) -> Self {
        Self { conn, room }
    }

    pub fn check_join_eligibility(
        &mut self,
        steam_id: &SteamID,
        ip: Option<String>,
    ) -> Result<JoinEligibility> {
        if !self.room.joinable {
            warn!("Game is not accepting new players!");
            return Ok(JoinEligibility::NotAccepting);
        }

        if self.room.has_started {
            warn!("Game has already started!");
            return Ok(JoinEligibility::AlreadyStarted);
        }

        if self.room.current_players >= self.room.max_players {
            warn!("Game is already full!");
            return Ok(JoinEligibility::Full);
        }

        if banned_players::table
            .count()
            .filter(
                banned_players::steam_id
                    .eq(steam_id)
                    .or(banned_players::ip.eq(ip)),
            )
            .get_result::<i64>(&mut self.conn)
            .unwrap_or_default()
            > 0
        {
            info!("The SteamID or IP address is banned!");
            return Ok(JoinEligibility::Banned);
        }

        if kicked_players::table
            .count()
            .filter(
                kicked_players::steam_id
                    .eq(steam_id)
                    .and(kicked_players::room_id.eq(self.room.id)),
            )
            .get_result::<i64>(&mut self.conn)
            .unwrap_or_default()
            > 0
        {
            info!("The SteamID was kicked from this game!");
            return Ok(JoinEligibility::Kicked);
        }

        Ok(JoinEligibility::Ok)
    }

    pub fn remove_game(mut self) -> Result<()> {
        delete(rooms::table)
            .filter(rooms::id.eq(self.room.id))
            .execute(&mut self.conn)?;

        Ok(())
    }

    pub fn update_host(&mut self, new_host: &SteamID) -> Result<()> {
        self.room = update(rooms::table)
            .filter(rooms::id.eq(self.room.id))
            .set(rooms::hosted_by.eq(new_host))
            .returning(Room::as_returning())
            .get_result(&mut self.conn)?;

        update(active_connections::table)
            .filter(
                active_connections::room_id
                    .eq(self.room.id)
                    .and(active_connections::steam_id.ne(new_host)),
            )
            .set(active_connections::is_host.eq(false))
            .execute(&mut self.conn)?;

        update(active_connections::table)
            .filter(
                active_connections::room_id
                    .eq(self.room.id)
                    .and(active_connections::steam_id.eq(new_host)),
            )
            .set(active_connections::is_host.eq(true))
            .execute(&mut self.conn)?;

        Ok(())
    }

    pub fn start(&mut self) -> Result<()> {
        self.room = update(rooms::table)
            .filter(rooms::id.eq(self.room.id))
            .set(rooms::has_started.eq(true))
            .returning(Room::as_returning())
            .get_result(&mut self.conn)?;

        Ok(())
    }

    pub fn update_settings(&mut self, settings: &GameSettings) -> Result<()> {
        self.room = update(rooms::table)
            .filter(rooms::id.eq(self.room.id))
            .set((
                rooms::max_players.eq(settings.max_players as i32),
                rooms::max_teams.eq(settings.max_teams as i32),
                rooms::team_composition.eq(settings.team_composition),
                rooms::grid_size.eq(settings.grid_size as i32),
                rooms::gamemode.eq(settings.game_mode),
                rooms::difficulty.eq(settings.difficulty),
                rooms::p_rank_required.eq(settings.p_rank_required),
                rooms::disable_campaign_alt_exits.eq(settings.disable_campaign_alt_exits),
                rooms::is_public.eq(settings.visibility == GameVisibility::Public),
            ))
            .returning(Room::as_returning())
            .get_result(&mut self.conn)?;

        Ok(())
    }

    pub fn set_joinable(&mut self, joinable: bool) -> Result<()> {
        self.room = update(rooms::table)
            .filter(rooms::id.eq(self.room.id))
            .set(rooms::joinable.eq(joinable))
            .returning(Room::as_returning())
            .get_result(&mut self.conn)?;

        Ok(())
    }

    pub fn update_player_count(&mut self, increment: i32) -> Result<()> {
        self.room = update(rooms::table)
            .filter(rooms::id.eq(self.room.id))
            .set(rooms::current_players.eq(self.room.current_players + increment))
            .returning(Room::as_returning())
            .get_result(&mut self.conn)?;

        Ok(())
    }

    pub fn get_player(&mut self, connection_hash: impl AsRef<str>) -> Result<ActiveConnection> {
        Ok(active_connections::table
            .filter(
                active_connections::room_id
                    .eq(self.room.id)
                    .and(active_connections::connection_hash.eq(connection_hash.as_ref())),
            )
            .select(ActiveConnection::as_select())
            .first(&mut self.conn)?)
    }

    pub fn get_players(&mut self) -> Result<Vec<ActiveConnection>> {
        Ok(active_connections::table
            .filter(active_connections::room_id.eq(self.room.id))
            .select(ActiveConnection::as_select())
            .load(&mut self.conn)?)
    }

    pub fn add_kick(&mut self, steam_id: &SteamID) -> Result<KickedPlayer> {
        Ok(insert_into(kicked_players::table)
            .values(KickedPlayerIn {
                steam_id: steam_id.clone(),
                room_id: self.room.id,
            })
            .returning(KickedPlayer::as_returning())
            .get_result(&mut self.conn)?)
    }

    pub fn clear_kicks(&mut self) -> Result<()> {
        delete(kicked_players::table)
            .filter(kicked_players::room_id.eq(self.room.id))
            .execute(&mut self.conn)?;

        Ok(())
    }

    pub fn check_player_count(&mut self) -> Result<i64> {
        Ok(active_connections::table
            .filter(
                active_connections::room_id
                    .eq(self.room.id)
                    .and(active_connections::is_host.eq(false)),
            )
            .count()
            .get_result(&mut self.conn)?)
    }

    pub fn mark_ended(&mut self) -> Result<()> {
        self.room = update(rooms::table)
            .filter(rooms::id.eq(self.room.id))
            .set(rooms::has_ended.eq(true))
            .returning(Room::as_returning())
            .get_result(&mut self.conn)?;

        Ok(())
    }
}
