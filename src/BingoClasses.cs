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
    public int timeToBeat;
    public int styleToBeat;
    
    public int row;
    public int column;
}

public class GameGrid
{
    public int size;
    public Dictionary<string,GameLevel> levelTable;
}

public class Game
{
    public int gameId;
    public Dictionary<string,Player> currentPlayers;
    
    public GameGrid grid;
    public string gameHost; //SteamID
    public int gameState;
    
    public List<Player> getPlayers()
    {
        return currentPlayers.Values.ToList();
    }
}