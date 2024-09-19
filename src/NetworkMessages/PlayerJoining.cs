using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class PlayerJoiningMessage : MessageResponse
{
    public string username;
}

public static class PlayerJoiningResponseHandler
{
    public static void handle(PlayerJoiningMessage response)
    {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(response.username + " has joined the game.");
    }
}