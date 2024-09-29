using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class TimeoutSignal : MessageResponse
{
    public string username;
    public string steamId;
}

public static class TimeoutSignalHandler
{
    public static void handle(TimeoutSignal response)
    {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(GameManager.CurrentGame.currentPlayers[response.steamId].username + " has lost connection to the game.");
        GameManager.CurrentGame.currentPlayers.Remove(response.steamId);
        GameManager.RefreshPlayerList();
    }
}