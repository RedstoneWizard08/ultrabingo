using System.Threading.Tasks;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class HostMigration : IncomingPacket {
    public required string OldHost;
    public required string HostSteamId;
    public required string HostUsername;

    public override Task Handle() {
        var message =
            $"The current host ({OldHost}) has lost connection.\n{(HostSteamId == Steamworks.SteamClient.SteamId.ToString()
                ? "You are now the new host."
                : $"The new host is now {HostUsername}.")}";

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(message);
        GameManager.CurrentGame.GameHost = HostSteamId;

        return Task.CompletedTask;
    }
}