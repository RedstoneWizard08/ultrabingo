using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("StartGame")]
public class StartGameRequest : BasePacket {
    [JsonProperty] public required int RoomId;
    [JsonProperty] public required RegisterTicket Ticket;
}