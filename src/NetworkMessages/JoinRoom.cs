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
    public static void handle(JoinRoomResponse response)
    {
        
        string msg = "Failed to join: ";
        
        if(response.status < 0)
        {
            switch(response.status)
            {
                case -5:
                {
                    msg += "<color=orange>You are banned from playing Baphomet's Bingo.</color>";
                    break;
                }
                case -4:
                {
                    msg += "Game has already started.";
                    break;
                }
                case -3:
                {
                    msg += "Game is not accepting new players.";
                    break;
                }
                case -2:
                {
                    msg += "Game has already started.";
                    break;
                }
                case -1:
                {
                    msg += "Game does not exist.";
                    break;
                }
            }
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
            NetworkManager.DisconnectWebSocket(1000,"Normal close");
        }
        else
        {
            GameManager.SetupGameDetails(response.roomDetails,false);
        }
    }
}