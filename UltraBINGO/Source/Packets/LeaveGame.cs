using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class LeaveGame : BasePacket {
    [JsonProperty] public required int RoomId;
    [JsonProperty] public required string Username;
    [JsonProperty] public required string SteamId;
}