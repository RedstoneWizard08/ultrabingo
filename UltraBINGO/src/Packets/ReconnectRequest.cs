using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class ReconnectRequest {
    public required int RoomId;
    public required string SteamId;
    public required RegisterTicket Ticket;
}