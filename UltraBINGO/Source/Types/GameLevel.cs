using Newtonsoft.Json;

namespace UltraBINGO.Types;

public class GameLevel {
    [JsonProperty] public required string LevelName;
    [JsonProperty] public required string LevelId;
    [JsonProperty] public required string ClaimedBy;
    [JsonProperty] public required string PersonToBeat;
    [JsonProperty] public required float TimeToBeat;
    [JsonProperty] public required int Row;
    [JsonProperty] public required int Column;
    [JsonProperty] public required bool IsAngryLevel;
    [JsonProperty] public required string AngryParentBundle;
    [JsonProperty] public required string AngryLevelId;
}