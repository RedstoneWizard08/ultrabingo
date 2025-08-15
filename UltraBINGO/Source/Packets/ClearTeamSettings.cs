using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("ClearTeams")]
public class ClearTeamSettings : BasePacket {
    [JsonProperty] public required int GameId;
    [JsonProperty] public required RegisterTicket Ticket;
}
