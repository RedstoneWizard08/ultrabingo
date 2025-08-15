using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("CreateRoom")]
public class CreateRoomRequest : BasePacket {
    [JsonProperty] public required string RoomName;
    [JsonProperty] public required string RoomPassword;
    [JsonProperty] public required int MaxPlayers;
    [JsonProperty] public required bool PRankRequired;
    [JsonProperty] public required string HostSteamName;
    [JsonProperty] public required string HostSteamId;
    [JsonProperty] public required string Rank;
}
