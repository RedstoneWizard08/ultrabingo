using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace UltraBINGO.Types;

public class Game {
    [JsonProperty] public required int GameId;
    [JsonProperty] public required Dictionary<string, Player> CurrentPlayers; //<SteamID,Player>
    [JsonProperty] public required GameGrid Grid;
    [JsonProperty] public string? GameHost; //SteamID
    [JsonProperty] public required int GameState;
    [JsonProperty] public required GameSettings GameSettings;
    public string? WinningTeam;

    public List<Player> GetPlayers() {
        return CurrentPlayers.Values.ToList();
    }

    public bool IsGameFinished() {
        return GameState == 2;
    }
}