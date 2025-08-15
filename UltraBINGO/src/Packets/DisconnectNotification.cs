using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.UI;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class DisconnectNotification : IncomingPacket {
    [JsonProperty] public required string Username;
    [JsonProperty] public required string SteamId;

    public override Task Handle() {
        var message = $"{Username} has left the game.";

        if (GameManager.PlayerIsHost() && BingoLobby.TeamComposition != null && BingoLobby.TeamComposition.value == 1)
            message += "\n Please recalibrate teams.";

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(message);
        GameManager.CurrentGame.CurrentPlayers.Remove(SteamId);
        GameManager.RefreshPlayerList();

        return Task.CompletedTask;
    }
}