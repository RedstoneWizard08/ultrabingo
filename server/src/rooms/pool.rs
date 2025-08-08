use crate::{
    db::DbPool,
    enums::{Difficulty, TeamComposition},
    models::{Room, RoomIn},
    modes::GameModeType,
    rooms::{manager::RoomManager, models::PublicGameData},
    schema::{active_connections, banned_players, ranks, rooms, user_ranks},
    types::SteamID,
    util::generate_code,
};
use anyhow::Result;
use diesel::{
    BoolExpressionMethods, ExpressionMethods, JoinOnDsl, NullableExpressionMethods, QueryDsl,
    RunQueryDsl, SelectableHelper, delete, insert_into,
};

pub struct PooledRoomManager {
    pub pool: DbPool,
}

impl PooledRoomManager {
    pub fn new(pool: DbPool) -> Self {
        Self { pool }
    }

    pub fn create(&self, steam_id: SteamID) -> Result<Room> {
        Ok(insert_into(rooms::table)
            .values(RoomIn {
                hosted_by: steam_id,
                room_code: generate_code(),
                current_players: 0,
                has_started: false,
                max_players: 8,
                max_teams: 4,
                team_composition: TeamComposition::Random,
                gamemode: GameModeType::Bingo,
                joinable: true,
                difficulty: Difficulty::Standard,
                disable_campaign_alt_exits: false,
                grid_size: 3,
                has_ended: false,
                is_public: false,
                p_rank_required: false,
            })
            .returning(Room::as_returning())
            .get_result(&mut self.pool.get()?)?)
    }

    pub fn get_game(&self, id: i32) -> Result<RoomManager> {
        let mut conn = self.pool.get()?;

        let room = rooms::table
            .filter(rooms::id.eq(id))
            .select(Room::as_select())
            .first(&mut conn)?;

        Ok(RoomManager::new(conn, room))
    }

    pub fn find_game(&self, code: impl AsRef<str>) -> Result<RoomManager> {
        let mut conn = self.pool.get()?;

        let room = rooms::table
            .filter(rooms::room_code.eq(code.as_ref().trim()))
            .select(Room::as_select())
            .first(&mut conn)?;

        Ok(RoomManager::new(conn, room))
    }

    pub fn clear_tables(&self) -> Result<()> {
        let mut conn = self.pool.get()?;

        delete(rooms::table).execute(&mut conn)?;
        delete(active_connections::table).execute(&mut conn)?;

        Ok(())
    }

    pub fn get_public_games(&self) -> Result<Vec<PublicGameData>> {
        Ok((rooms::table
            .left_join(
                active_connections::table.on(rooms::hosted_by.eq(active_connections::steam_id)),
            )
            .filter(
                rooms::has_started
                    .eq(false)
                    .and(rooms::is_public.eq(true))
                    .and(active_connections::room_id.eq(rooms::id)),
            )
            .select((Room::as_select(), active_connections::username.nullable()))
            .load::<(Room, Option<String>)>(&mut self.pool.get()?)?
            as Vec<(Room, Option<String>)>)
            .into_iter()
            .map(|(room, user)| PublicGameData {
                current_players: room.current_players as usize,
                difficulty: room.difficulty,
                max_players: room.max_players as usize,
                password: room.room_code,
                username: user.unwrap(),
            })
            .collect())
    }

    pub fn fetch_available_ranks(&self, steam_id: SteamID) -> Result<Vec<String>> {
        Ok(ranks::table
            .left_join(user_ranks::table.on(ranks::id.eq(user_ranks::rank_id)))
            .filter(user_ranks::steam_id.eq(&steam_id))
            .select(ranks::name)
            .load(&mut self.pool.get()?)?)
    }

    pub fn check_ban(&self, steam_id: SteamID, ip: impl AsRef<str>) -> Result<bool> {
        Ok(banned_players::table
            .count()
            .filter(
                banned_players::steam_id
                    .eq(steam_id)
                    .or(banned_players::ip.eq(ip.as_ref().to_string())),
            )
            .get_result::<i64>(&mut self.pool.get()?)
            .unwrap_or_default()
            > 0)
    }
}
