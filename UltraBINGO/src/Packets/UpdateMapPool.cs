using System.Collections.Generic;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class UpdateMapPool : BasePacket {
    public required int GameId;
    public required List<string> MapPoolIds;
    public required RegisterTicket Ticket;
}