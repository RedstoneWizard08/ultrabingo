use crate::{
    game::{GameInfo, models::SubmissionData},
    messages::{GameEndStatus, OutgoingMessage, SubmissionResult},
    modes::GameMode,
    rooms::manager::RoomManager,
    server::HttpState,
    util::format_time_delta,
};
use anyhow::Result;
use chrono::Utc;
use itertools::Itertools;
use std::{collections::HashMap, sync::Arc, time::Duration};

#[derive(Debug, Clone, Copy)]
pub struct DominationGamemode;

impl DominationGamemode {
    pub fn force_end(
        game: &mut GameInfo,
        room: &mut RoomManager,
        _data: Option<SubmissionData>,
    ) -> Result<()> {
        game.ended = true;

        let mut tracker: HashMap<String, usize> = HashMap::new();

        for name in &game.team_names {
            tracker.insert(name.clone(), 0);
        }

        for level in game.grid.level_table.iter().flatten() {
            if let Some(team) = &level.claimed_by_team {
                *tracker.get_mut(team).unwrap() += 1;
            }
        }

        let mut end_status = GameEndStatus::Win;

        let max_value = tracker
            .iter()
            .max_by_key(|(_, value)| **value)
            .map(|(_, value)| *value)
            .unwrap_or(0);

        let mut winning = tracker
            .iter()
            .max_by_key(|(_, value)| **value)
            .map(|(name, _)| name.clone())
            .unwrap_or(tracker.keys().next().unwrap().clone());

        let mut winning_players = game.teams[&winning].clone();
        let mut tied_teams = Vec::new();

        if game.num_claims == 0 {
            warn!("No claims were made during this game!");

            winning = "NONE".into();
            end_status = GameEndStatus::NoClaims;
            winning_players.clear();
        } else if !tracker
            .values()
            .filter(|it| **it == max_value)
            .collect_vec()
            .is_empty()
        {
            info!("TIE");

            tied_teams = tracker
                .iter()
                .filter(|(_, value)| **value == max_value)
                .map(|(name, _)| name.clone())
                .collect_vec();

            end_status = GameEndStatus::Tie;
            winning_players = Vec::new();
        } else {
            info!("Winning team: {winning}");
        }

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
            winning_team: winning,
            winning_players: winning_players.iter().map(|it| format!("{}", it)).collect(),
            time_elapsed: elapsed,
            claims: game.num_claims,
            first_map_claimed: game.first_map_claimed.clone(),
            last_map_claimed: game.last_map_claimed.clone(),
            best_stat_value: game.best_stat_value,
            best_stat_map: game.best_stat_map.clone(),
            end_status,
            tied_teams,
        })?;

        Ok(())
    }
}

impl GameMode for DominationGamemode {
    fn setup(&mut self, game: &mut GameInfo, state: &HttpState) -> Result<()> {
        info!("Setting up Domination gamemode");

        let time = Duration::from_secs(game.settings.time_limit as u64 * 60);
        let game_id = game.id;
        let state = Arc::clone(state);

        tokio::task::spawn(async move {
            tokio::time::sleep(time).await;

            info!("Time up!");

            let state = state.read();

            if let Some(game) = state.current_games.get(&game_id) {
                if let Ok(mut room) = state.rooms.get_game(game_id) {
                    let mut game = game.write();
                    let mut info = game.info();

                    if let Err(err) = DominationGamemode::force_end(&mut info, &mut room, None) {
                        error!("Failed to end domination gamemode for game {game_id}: {err}");
                    }

                    game.apply(info);
                } else {
                    error!("Failed to get room for game {game_id} after timer ended!");
                }
            } else {
                error!("Failed to get game {game_id} after timer ended!");
            }
        });

        info!("Timer started at {} minutes", game.settings.time_limit);

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
            level,
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
        Self::force_end(game, room, Some(received))
    }
}
