using System.Collections.Generic;
using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("UpdateTeamSettings")]
public class TeamSettings : BasePacket {
    [JsonProperty] public required int GameId;
    [JsonProperty] public required Dictionary<string, int> Teams;
    [JsonProperty] public required RegisterTicket Ticket;
}