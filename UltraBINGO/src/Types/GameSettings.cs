using Newtonsoft.Json;

namespace UltraBINGO.Types;

public class GameSettings {
    [JsonProperty] public int MaxPlayers;
    [JsonProperty] public int MaxTeams;
    [JsonProperty] public int TimeLimit;
    [JsonProperty] public int TeamComposition;
    [JsonProperty] public int Gamemode;
    [JsonProperty] public int GridSize;
    [JsonProperty] public int Difficulty;
    [JsonProperty] public bool RequiresPRank;
    [JsonProperty] public bool HasManuallySetTeams;
    [JsonProperty] public bool DisableCampaignAltExits;
    [JsonProperty] public int GameVisibility;
    [JsonProperty] public int DominationTimer;
}