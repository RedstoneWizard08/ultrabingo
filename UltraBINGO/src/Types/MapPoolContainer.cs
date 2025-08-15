using System.Collections.Generic;
using Newtonsoft.Json;

namespace UltraBINGO.Types;

public class MapPoolContainer(
    string mapPoolId,
    string mapPoolName,
    string description,
    int numOfMaps,
    List<string> mapList
) {
    [JsonConstructor]
    private MapPoolContainer() : this("", "", "", -1, []) {
    }

    [JsonProperty] public string MapPoolId = mapPoolId;
    [JsonProperty] public string MapPoolName = mapPoolName;
    [JsonProperty] public string Description = description;
    [JsonProperty] public int NumOfMaps = numOfMaps;
    [JsonProperty] public List<string> MapList = mapList;
}