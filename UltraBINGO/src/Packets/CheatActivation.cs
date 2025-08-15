using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class CheatActivation : BasePacket {
    [JsonProperty] public required int GameId;
    [JsonProperty] public required string Username;
    [JsonProperty] public required string SteamId;
}
