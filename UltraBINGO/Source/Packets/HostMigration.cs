using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class HostMigration : IncomingPacket {
    [JsonProperty] public required string OldHost;
    [JsonProperty] public required string HostSteamId;
    [JsonProperty] public required string HostUsername;

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