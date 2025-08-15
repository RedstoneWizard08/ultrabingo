using Newtonsoft.Json;

namespace UltraBINGO.Types;

public class PublicGameData {
    [JsonProperty] public required string Password;
    [JsonProperty] public required int CurrentPlayers;
    [JsonProperty] public required string HostUsername;
    [JsonProperty] public required int MaxPlayers;
    [JsonProperty] public required int Difficulty;
}
