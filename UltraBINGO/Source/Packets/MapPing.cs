using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class MapPing : BasePacket {
    [JsonProperty] public required int GameId;
    [JsonProperty] public required string Team;
    [JsonProperty] public required int Row;
    [JsonProperty] public required int Column;
    [JsonProperty] public required RegisterTicket Ticket;
}