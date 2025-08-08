using System.Threading.Tasks;

namespace UltraBINGO.NetworkMessages;

public class ReconnectRequest : SendMessage {
    public string messageType = "ReconnectRequest";

    public int roomId;
    public string steamId;
    public RegisterTicket ticket;
}

public class ReconnectResponse : MessageResponse {
    public string status;
    public Game gameData;
}

public static class ReconnectResponseHandler {
    public static async void Handle(ReconnectResponse response) {
        switch (response.status) {
            case "OK": {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Reconnected successfully.");
                break;
            }
            case "end": {
                var dc = new DisconnectSignal {
                    disconnectMessage = "GAMEENDED"
                };
                DisconnectSignalHandler.Handle(dc);
                //MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("The game has already ended.");
                break;
            }
            default: {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                    "Failed to reconnect. Leaving game in 5 seconds...");
                GameManager.ClearGameVariables();
                await Task.Delay(5000);

                SceneHelper.LoadScene("Main Menu");
                break;
            }
        }
    }
}