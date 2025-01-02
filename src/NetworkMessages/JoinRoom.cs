using System.Collections.Generic;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class JoinRoomRequest : SendMessage
{
    public string messageType = "JoinRoom";
    
    public int roomId;
    public string username;
    public string steamId;
    
}

public class JoinRoomResponse : MessageResponse
{
    public int status;
    public int roomId;
    public Game roomDetails;
}

public static class JoinRoomResponseHandler
{
    public static Dictionary<int,string> messages = new Dictionary<int, string>()
    {
        {-6,"You have been kicked from this game."},
        {-5, "<color=orange>You are banned from playing Baphomet's Bingo.</color>"},
        {-4, "Game has already started."},
        {-3, "Game is not accepting new players."},
        {-2, "Game has already started."},
        {-1, "Game does not exist."},
    };

    public static void handle(JoinRoomResponse response)
    {
        string msg = "Failed to join: ";
        
        if(response.status < 0)
        {
            msg += messages[response.status];
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
            NetworkManager.DisconnectWebSocket(1000,"Normal close");
        }
        else
        {
            GameManager.SetupGameDetails(response.roomDetails,false);
        }
    }
}