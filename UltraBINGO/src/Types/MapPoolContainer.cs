using System.Collections.Generic;

namespace UltraBINGO.Types;

public class MapPoolContainer {
    public string MapPoolId;
    public string MapPoolName;
    public string Description;
    public int NumOfMaps;
    public List<string> MapList;

    public MapPoolContainer(string mapPoolId, string mapPoolName, string description, int numOfMaps,
        List<string> mapList) {
        this.MapPoolId = mapPoolId;
        this.MapPoolName = mapPoolName;
        this.Description = description;
        this.NumOfMaps = numOfMaps;
        this.MapList = mapList;
    }
}