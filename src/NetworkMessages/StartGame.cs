using System.Collections.Generic;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class StartGameRequest : SendMessage
{
    public string messageType = "StartGame";
    
    public int roomId;
    
}

public class StartGameResponse : MessageResponse
{
    public string teamColor;
    public List<string> teammates;
    
    public GameGrid grid;
}

public static class StartGameResponseHandler
{
    public static void handle(StartGameResponse response)
    {
        GameManager.currentTeam = response.teamColor;
        GameManager.teammates = response.teammates;
        GameManager.CurrentGame.grid = response.grid;
        Logging.Message("We are on the "+GameManager.currentTeam + " team");
                
        BingoMenuController.StartGame();
    }
}