using System;
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
    public Game game;
    public string teamColor;
    public List<string> teammates;
    
    public GameGrid grid;
}

public static class StartGameResponseHandler
{
    public static void handle(StartGameResponse response)
    {
        GameManager.CurrentTeam = response.teamColor;
        GameManager.Teammates = response.teammates;
        GameManager.CurrentGame.grid = response.grid;
        Logging.Message("We are on the "+GameManager.CurrentTeam + " team");
                
        BingoMenuController.StartGame();
    }
}