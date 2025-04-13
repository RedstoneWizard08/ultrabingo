using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class PlayerJoiningMessage : MessageResponse
{
    public string username;
    public string steamId;
    public string rank;
}

public static class PlayerJoiningResponseHandler
{
    public static void handle(PlayerJoiningMessage response)
    {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(response.username + " has joined the game.");
        Player newPlayer = new Player();
        newPlayer.username = response.username;
        newPlayer.rank = response.rank;
        GameManager.CurrentGame.currentPlayers[response.steamId] = newPlayer;
        GameManager.RefreshPlayerList();
    }
}