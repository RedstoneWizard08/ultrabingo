using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class PlayerJoiningMessage : MessageResponse
{
    public string username;
    public string steamId;
}

public static class PlayerJoiningResponseHandler
{
    public static void handle(PlayerJoiningMessage response)
    {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(response.username + " has joined the game.");
        Player newPlayer = new Player();
        newPlayer.username = response.username;
        GameManager.CurrentGame.currentPlayers[response.steamId] = newPlayer;
        GameManager.RefreshPlayerList();
    }
}