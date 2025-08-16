using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.UI;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class ServerDisconnection : IncomingPacket {
    [JsonProperty] public required int DisconnectCode;
    [JsonProperty] public required string DisconnectMessage;

    public override async void Handle() {
        var disconnectReason = DisconnectMessage switch {
            "HOSTLEFTGAME" => "The host has left the game. The game will be ended.",
            "HOSTDROPPED" => "The host has lost connection. The game will be ended.",
            "RECONNECTFAILED" => "Failed to reconnect.",
            "GAMEENDED" => "The game has already ended.",
            _ => "Disconnected for an unspecified reason (check console).\nThe game will be ended."
        };

        GameManager.ClearGameVariables();

        //If the player is in-game, warn them of returning to menu in 5 seconds.
        if (GetSceneName() == "Main Menu") {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(disconnectReason);
            BingoEncapsulator.BingoCardScreen?.SetActive(false);
            BingoEncapsulator.BingoLobbyScreen?.SetActive(false);
            BingoEncapsulator.BingoEndScreen?.SetActive(false);
            BingoEncapsulator.BingoMenu?.SetActive(true);
        } else {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage($"{disconnectReason}\nExiting in 5 seconds...");
            await Task.Delay(5000);
            SceneHelper.LoadScene("Main Menu");
        }
    }
}