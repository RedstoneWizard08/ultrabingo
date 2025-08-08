namespace UltraBINGO.NetworkMessages;

public class HostMigration : MessageResponse {
    public string oldHost;

    public string hostSteamId;
    public string hostUsername;
}

public static class HostMigrationHandler {
    public static void Handle(HostMigration response) {
        var message =
            "The current host (" + response.oldHost + ") has lost connection.\n"
            + (response.hostSteamId == Steamworks.SteamClient.SteamId.ToString()
                ? "You are now the new host."
                : "The new host is now " + response.hostUsername + ".");

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(message);

        GameManager.CurrentGame.gameHost = response.hostSteamId;
    }
}