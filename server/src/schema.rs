// @generated automatically by Diesel CLI.

diesel::table! {
    active_connections (id) {
        id -> Integer,
        connection_hash -> Text,
        ticket -> Text,
        steam_id -> BigInt,
        username -> Text,
        room_id -> Integer,
        is_host -> Bool,
    }
}

diesel::table! {
    banned_players (id) {
        id -> Integer,
        steam_id -> BigInt,
        ip -> Nullable<Text>,
    }
}

diesel::table! {
    kicked_players (id) {
        id -> Integer,
        steam_id -> BigInt,
        room_id -> Integer,
    }
}

diesel::table! {
    ranks (id) {
        id -> Integer,
        name -> Text,
    }
}

diesel::table! {
    rooms (id) {
        id -> Integer,
        room_code -> Text,
        hosted_by -> BigInt,
        current_players -> Integer,
        has_started -> Bool,
        max_players -> Integer,
        max_teams -> Integer,
        team_composition -> SmallInt,
        joinable -> Bool,
        grid_size -> Integer,
        gamemode -> SmallInt,
        difficulty -> SmallInt,
        p_rank_required -> Bool,
        disable_campaign_alt_exits -> Bool,
        is_public -> Bool,
        has_ended -> Bool,
    }
}

diesel::table! {
    user_ranks (id) {
        id -> Integer,
        steam_id -> BigInt,
        rank_id -> Integer,
    }
}

diesel::joinable!(active_connections -> rooms (room_id));
diesel::joinable!(kicked_players -> rooms (room_id));
diesel::joinable!(user_ranks -> ranks (rank_id));

diesel::allow_tables_to_appear_in_same_query!(
    active_connections,
    banned_players,
    kicked_players,
    ranks,
    rooms,
    user_ranks,
);
