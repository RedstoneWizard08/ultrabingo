using System.Collections.Generic;
using System.Linq;

namespace UltraBINGO;

public class Player
{
    public string username;
}

public class GameLevel
{
    public string levelName;
    public string claimedBy;
    public string personToBeat;
    public float timeToBeat;
    public int styleToBeat;
    
    public int row;
    public int column;
}

public class GameGrid
{
    public int size;
    public Dictionary<string,GameLevel> levelTable;
}

public class GameSettings
{
    public int maxPlayers;
    public int maxTeams;
    public bool requiresPRank;
    public int gameType;
    public int difficulty;
}

public class Game
{
    public int gameId;
    public Dictionary<string,Player> currentPlayers; //<SteamID,Player>
    
    public GameGrid grid;
    public string gameHost; //SteamID
    public int gameState;
    
    public GameSettings gameSettings;
    
    public string winningTeam;
    
    public List<Player> getPlayers()
    {
        return currentPlayers.Values.ToList();
    }
    
    public bool isGameFinished()
    {
        return this.gameState == 2; // State 2 = game finished
    }
}