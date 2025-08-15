using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("ClearTeams")]
public class ClearTeamSettings : BasePacket {
    public required int GameId;
    public required RegisterTicket Ticket;
}
