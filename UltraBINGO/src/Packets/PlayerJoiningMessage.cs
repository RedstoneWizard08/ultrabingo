using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.Types;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class PlayerJoiningMessage : IncomingPacket {
    [JsonProperty] public required string Username;
    [JsonProperty] public required string SteamId;
    [JsonProperty] public required string Rank;

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