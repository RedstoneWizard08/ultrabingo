using UltraBINGO.UI_Elements;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class UpdateRoomSettingsRequest : SendMessage
{
    public string messageType = "UpdateRoomSettings";
    
    public int roomId;
    
    public int maxPlayers;
    public int maxTeams;
    public bool PRankRequired;
    public int gameType;
    public int difficulty;
    public int levelRotation;
}

public class UpdateRoomSettingsNotification : MessageResponse
{
    public int maxPlayers;
    public int maxTeams;
    public bool PRankRequired;
    public int gameType;
    public int difficulty;
    public int levelRotation;
}

public static class UpdateRoomSettingsHandler
{
    public static void handle(UpdateRoomSettingsNotification response)
    {
        BingoLobby.updateFromNotification(response);
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("The host has updated the room settings.");
    }
}