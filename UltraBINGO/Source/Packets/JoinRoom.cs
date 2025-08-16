using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class JoinRoom : BasePacket {
    [JsonProperty] public required string Password;
    [JsonProperty] public required string Username;
    [JsonProperty] public required string SteamId;
    [JsonProperty] public required string Rank;
}