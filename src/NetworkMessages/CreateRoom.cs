using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class CreateRoomRequest : SendMessage
{
    public string messageType = "CreateRoom";
    
    public string roomName;
    public string roomPassword;
    public int maxPlayers;
    
    public short gameType; //1 = Based on time, 2 = Based on style;
    public bool pRankRequired;
    
    public string hostSteamName;
    public string hostSteamId;
}

public class CreateRoomResponse : MessageResponse
{
    public string status;
    public int roomId;
    public Game roomDetails;
    public string roomPassword;
}

public static class CreateRoomResponseHandler
{
    public static void handle(CreateRoomResponse response)
    {
        if(response.status == "ban")
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=orange>You have been banned from playing Baphomet's Bingo.</color>");
        }
        else
        {
            if(response.roomId == 0)
            {
                //Was unable to create room
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to create room.");
            }
            else
            {
                Logging.Message("Got details for room "+response.roomId);
                
                //Once room details have been obtained: set up the lobby screen
                GameManager.SetupGameDetails(response.roomDetails,response.roomPassword);
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Joined game: "+response.roomId);
            } 
        }
    }
}