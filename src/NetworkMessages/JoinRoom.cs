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
        if(response.status < 0)
        {
            switch(response.status)
            {
                case -3:
                {
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to join: Game does not exist.");
                    break;
                }
                case -2:
                {
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to join: Game is full.");
                    break;
                }
                case -1:
                {
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to join: Game has already started.");
                    break;
                }
            }
            NetworkManager.DisconnectWebSocket(1000,"Normal close");
        }
        else
        {
            GameManager.SetupGameDetails(response.roomDetails,false);
        }
    }
}