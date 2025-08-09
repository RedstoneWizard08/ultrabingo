using System.Threading.Tasks;
using UltraBINGO.API;
using UltraBINGO.UI;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class DisconnectNotification : IncomingPacket {
    public required string Username;
    public required string SteamId;

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