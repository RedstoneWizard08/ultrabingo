using System.Collections.Generic;
using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class UpdateMapPool : BasePacket {
    [JsonProperty] public required int GameId;
    [JsonProperty] public required List<string> MapPoolIds;
    [JsonProperty] public required RegisterTicket Ticket;
}