using System.Collections.Generic;

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
    public List<Player> currentPlayers;
    public GameGrid grid;
    public Player gameHost;
    public int gameState;
    
    public List<Player> getPlayers()
    {
        return currentPlayers;
    }
}