using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnassignedField.Global
// ReSharper disable ConvertToPrimaryConstructor
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace UltraBINGO;

public enum State {
    NORMAL,
    INMENU,
    INLOBBY,
    INBROWSER,
    INGAME
}

public enum AsyncAction {
    None,
    Host,
    Join,
    ModCheck,
    RetrySend,
    ReconnectGame,
    FetchGames
}

public class SendMessage {
    public string messageType = null!;
}

public class MessageResponse {
    public string messageType = null!;
}

public class PlayerNotification {
    public string messageType = null!;
}

public class MapPoolContainer {
    public string mapPoolId;
    public string mapPoolName;
    public string description;
    public int numOfMaps;
    public List<string> mapList;

    public MapPoolContainer(string mapPoolId, string mapPoolName, string description, int numOfMaps,
        List<string> mapList) {
        this.mapPoolId = mapPoolId;
        this.mapPoolName = mapPoolName;
        this.description = description;
        this.numOfMaps = numOfMaps;
        this.mapList = mapList;
    }
}

public class Player {
    public string username;
    public string steamId;
    public string rank;
}

public class GameLevel {
    public string levelName;
    public string levelId;

    public string claimedBy;
    public string personToBeat;
    public float timeToBeat;

    public int row;
    public int column;

    public bool isAngryLevel;
    public string angryParentBundle;
    public string angryLevelId;
}

public class GameGrid {
    public int size;
    public Dictionary<string, GameLevel> levelTable;
}

public class GameSettings {
    public int maxPlayers;
    public int maxTeams;
    public int timeLimit;
    public int teamComposition;
    public int gamemode;
    public int gridSize;
    public int difficulty;
    public bool requiresPRank;
    public bool hasManuallySetTeams;
    public bool disableCampaignAltExits;
    public int gameVisibility;

    public int dominationTimer;
}

public class Game {
    public int gameId;
    public Dictionary<string, Player> currentPlayers; //<SteamID,Player>

    public GameGrid grid;
    public string gameHost; //SteamID
    public int gameState;

    public GameSettings gameSettings;

    public string winningTeam;

    public List<Player> getPlayers() {
        return currentPlayers.Values.ToList();
    }

    public bool isGameFinished() {
        return gameState == 2;
    }
}