using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class TimeoutSignal : MessageResponse
{
    public string username;
}

public static class TimeoutSignalHandler
{
    public static void handle(TimeoutSignal response)
    {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(response.username + " has lost connection to the game.");
    }
}