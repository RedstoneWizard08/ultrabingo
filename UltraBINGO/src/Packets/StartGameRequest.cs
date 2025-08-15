using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("StartGame")]
public class StartGameRequest : BasePacket {
    public required int RoomId;
    public required RegisterTicket Ticket;
}