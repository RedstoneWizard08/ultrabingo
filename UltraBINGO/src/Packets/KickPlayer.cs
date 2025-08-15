using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class KickPlayer : BasePacket {
    public required int GameId;
    public required string PlayerToKick;
    public required RegisterTicket Ticket;
}