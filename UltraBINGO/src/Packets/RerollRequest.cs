using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class RerollRequest : BasePacket {
    [JsonProperty] public required RegisterTicket SteamTicket;
    [JsonProperty] public required string SteamId;
    [JsonProperty] public required int GameId;
    [JsonProperty] public required int Row;
    [JsonProperty] public required int Column;
}