using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class UpdateRoomSettings : BasePacket {
    [JsonProperty] public required int RoomId;
    [JsonProperty] public required int MaxPlayers;
    [JsonProperty] public required int MaxTeams;
    [JsonProperty] public required int TimeLimit;
    [JsonProperty] public required int TeamComposition;
    [JsonProperty] public required int GameMode;
    [JsonProperty] public required bool PRankRequired;
    [JsonProperty] public required int Difficulty;
    [JsonProperty] public required int GridSize;
    [JsonProperty] public required bool DisableCampaignAltExits;
    [JsonProperty] public required int GameVisibility;
    [JsonProperty] public required RegisterTicket Ticket;
}