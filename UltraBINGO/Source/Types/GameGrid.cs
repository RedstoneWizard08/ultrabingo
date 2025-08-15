using System.Collections.Generic;
using Newtonsoft.Json;

namespace UltraBINGO.Types;

public class GameGrid {
    [JsonProperty] public required int Size;
    [JsonProperty] public required Dictionary<string, GameLevel> LevelTable;
}