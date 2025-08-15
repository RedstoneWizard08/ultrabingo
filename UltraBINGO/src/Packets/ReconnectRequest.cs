using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class ReconnectRequest : BasePacket {
    [JsonProperty] public required int RoomId;
    [JsonProperty] public required string SteamId;
    [JsonProperty] public required RegisterTicket Ticket;
}