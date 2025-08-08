CREATE TABLE IF NOT EXISTS rooms (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    room_code TEXT NOT NULL,
    hosted_by BIGINT NOT NULL,
    current_players INTEGER NOT NULL,
    has_started BOOLEAN NOT NULL,
    max_players INTEGER NOT NULL,
    max_teams INTEGER NOT NULL,
    team_composition SMALLINT NOT NULL,
    joinable BOOLEAN NOT NULL,
    grid_size INTEGER NOT NULL,
    gamemode SMALLINT NOT NULL DEFAULT 0,
    difficulty SMALLINT NOT NULL,
    p_rank_required BOOLEAN NOT NULL,
    disable_campaign_alt_exits BOOLEAN NOT NULL,
    is_public BOOLEAN NOT NULL DEFAULT FALSE,
    has_ended BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS active_connections (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    connection_hash TEXT NOT NULL,
    ticket TEXT NOT NULL,
    steam_id BIGINT NOT NULL,
    username TEXT NOT NULL,
    room_id INTEGER NOT NULL REFERENCES rooms (id),
    is_host BOOLEAN NOT NULL
);

CREATE TABLE IF NOT EXISTS kicked_players (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    steam_id BIGINT NOT NULL,
    room_id INTEGER NOT NULL REFERENCES rooms (id)
);

CREATE TABLE IF NOT EXISTS banned_players (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    steam_id BIGINT NOT NULL,
    ip TEXT
);

CREATE TABLE IF NOT EXISTS ranks (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL
);

DELETE FROM ranks;

INSERT INTO ranks (name) VALUES
    ('<color=#e74c3c>DEVELOPER</color>'),
    ('<color=#f1c40f>TESTER</color>'),
    ('<color=green>DONATOR</color>'),
    ('<color=#35a8ff>LEGACY MAPPER</color>'),
    ('<color=#35a8ff>MAPPER</color>'),
    ('<color=#ffce84>MODDER</color>'),
    ('<color=#aea8ff>SPEEDRUNNER</color>'),
    ('<color=red>NEW BLOOD</color>');

CREATE TABLE IF NOT EXISTS user_ranks (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    steam_id BIGINT NOT NULL,
    rank_id INTEGER NOT NULL REFERENCES ranks (id)
);
