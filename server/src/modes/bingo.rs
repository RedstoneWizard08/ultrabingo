use anyhow::Result;
use chrono::Utc;

use crate::{
    game::{GameInfo, models::SubmissionData},
    messages::{GameEndStatus, OutgoingMessage, SubmissionResult},
    modes::GameMode,
    rooms::manager::RoomManager,
    server::HttpState,
    util::format_time_delta,
};

#[derive(Debug, Clone, Copy)]
pub struct BingoGamemode;

impl GameMode for BingoGamemode {
    fn setup(&mut self, _game: &mut GameInfo, _state: &HttpState) -> Result<()> {
        info!("Setting up Bingo gamemode");
        Ok(())
    }

    fn on_map_claim(
        &mut self,
        game: &mut GameInfo,
        room: &mut RoomManager,
        received: SubmissionData,
        submit_result: SubmissionResult,
        map_is_being_voted: bool,
    ) -> Result<()> {
        let obtained = game.check_bingo(&received.team, received.row, received.column);

        let level = game.grid.level_table[received.row][received.column]
            .name
            .clone();

        info!("Notifying all players in game");

        game.broadcast(OutgoingMessage::LevelClaimed {
            username: received.player_name.clone(),
            team: received.team.clone(),
            level_name: level,
            claim_type: submit_result,
            row: received.row,
            column: received.column,
            new_time_requirement: received.time,
            is_map_voted: map_is_being_voted,
        })?;

        if obtained {
            self.end_game(game, room, received)?;
        }

        Ok(())
    }

    fn end_game(
        &mut self,
        game: &mut GameInfo,
        room: &mut RoomManager,
        received: SubmissionData,
    ) -> Result<()> {
        game.ended = true;

        let winners = &game.teams[&received.team];
        let end_time = Utc::now();

        info!(
            "Ending game {} at {}",
            game.id,
            end_time.format("%Y-%m-%d %H:%M:%S %Z")
        );

        let elapsed = format_time_delta(game.start_time.unwrap_or(Utc::now()) - end_time);

        info!("Elapsed time of game: {}", elapsed);

        room.mark_ended()?;

        game.broadcast(OutgoingMessage::GameEnd {
            winning_team: received.team,
            winning_players: winners.iter().map(|it| format!("{}", it)).collect(),
            time_elapsed: elapsed,
            claims: game.num_claims,
            first_map_claimed: game.first_map_claimed.clone(),
            last_map_claimed: game.last_map_claimed.clone(),
            best_stat_value: game.best_stat_value,
            best_stat_map: game.best_stat_map.clone(),
            end_status: GameEndStatus::Win,
            tied_teams: Vec::new(),
        })?;

        Ok(())
    }
}
