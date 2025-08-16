using System.Collections.Generic;

namespace UltraBINGO.Types;

public class MapPoolContainer(
    string mapPoolId,
    string mapPoolName,
    string description,
    int numOfMaps,
    List<string> mapList
) {
    public readonly string MapPoolId = mapPoolId;
    public readonly string MapPoolName = mapPoolName;
    public readonly string Description = description;
    public readonly int NumOfMaps = numOfMaps;
    public readonly List<string> MapList = mapList;
}