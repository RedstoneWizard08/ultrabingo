using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class MapPing : BasePacket {
    public required int GameId;
    public required string Team;
    public required int Row;
    public required int Column;
    public required RegisterTicket Ticket;
}