using System.Collections.Generic;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("UpdateTeamSettings")]
public class TeamSettings {
    public required int GameId;
    public required Dictionary<string, int> Teams;
    public required RegisterTicket Ticket;
}