using Newtonsoft.Json;

namespace UltraBINGO.Types;

public class Player {
    [JsonProperty] public required string Username;
    [JsonProperty] public required string SteamId;
    [JsonProperty] public required string Rank;
}