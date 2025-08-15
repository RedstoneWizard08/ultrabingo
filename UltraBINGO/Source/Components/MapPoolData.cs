using System.Collections.Generic;
using UnityEngine;

namespace UltraBINGO.Components;

public class MapPoolData : MonoBehaviour {
    public string mapPoolId = "id";
    public string mapPoolName = "MapPoolName";
    public string mapPoolDescription = "Desc";
    public bool mapPoolEnabled;
    public int mapPoolNumOfMaps;
    public List<string>? mapPoolMapList;

    public void Toggle() {
        mapPoolEnabled = !mapPoolEnabled;
    }
}