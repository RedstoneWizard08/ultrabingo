using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("JoinRoom")]
public class JoinRoomRequest : BasePacket {
    [JsonProperty] public required string Password;
    [JsonProperty] public required string Username;
    [JsonProperty] public required string SteamId;
    [JsonProperty] public required string Rank;
}