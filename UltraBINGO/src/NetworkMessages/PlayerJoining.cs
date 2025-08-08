namespace UltraBINGO.NetworkMessages;

public class PlayerJoiningMessage : MessageResponse {
    public string username;
    public string steamId;
    public string rank;
}

public static class PlayerJoiningResponseHandler {
    public static void Handle(PlayerJoiningMessage response) {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(response.username + " has joined the game.");
        var newPlayer = new Player {
            username = response.username,
            rank = response.rank
        };
        GameManager.CurrentGame.currentPlayers[response.steamId] = newPlayer;
        GameManager.RefreshPlayerList();
    }
}