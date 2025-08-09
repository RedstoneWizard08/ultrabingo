using System.Threading.Tasks;
using UltraBINGO.API;
using UltraBINGO.Types;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class PlayerJoiningMessage : IncomingPacket {
    public required string Username;
    public required string SteamId;
    public required string Rank;

    public override Task Handle() {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage($"{Username} has joined the game.");

        var newPlayer = new Player {
            Username = Username,
            Rank = Rank,
            SteamId = SteamId,
        };

        GameManager.CurrentGame.CurrentPlayers[SteamId] = newPlayer;
        GameManager.RefreshPlayerList();

        return Task.CompletedTask;
    }
}