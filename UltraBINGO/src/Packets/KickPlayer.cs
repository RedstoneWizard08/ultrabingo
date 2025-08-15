using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class KickPlayer : BasePacket {
    [JsonProperty] public required int GameId;
    [JsonProperty] public required string PlayerToKick;
    [JsonProperty] public required RegisterTicket Ticket;
}