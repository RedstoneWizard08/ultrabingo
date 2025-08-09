using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("StartGame")]
public class StartGameRequest {
    public required int RoomId;
    public required RegisterTicket Ticket;
}