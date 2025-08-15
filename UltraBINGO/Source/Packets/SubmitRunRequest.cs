using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("SubmitRun")]
public class SubmitRunRequest : BasePacket {
    [JsonProperty] public required string Team;
    [JsonProperty] public required int GameId;
    [JsonProperty] public required int Column;
    [JsonProperty] public required int Row;
    [JsonProperty] public required string LevelName;
    [JsonProperty] public required string LevelId;
    [JsonProperty] public required string PlayerName;
    [JsonProperty] public required string SteamId;
    [JsonProperty] public required float Time;
    [JsonProperty] public required RegisterTicket Ticket;
}