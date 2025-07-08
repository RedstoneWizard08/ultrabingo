using System.Collections.Generic;
using UnityEngine;

namespace UltraBINGO.Components;

public class MapPoolData : MonoBehaviour
{
    public int mapPoolId = 0;
    public string mapPoolName = "MapPoolName";
    public string mapPoolDescription = "Desc";
    public bool mapPoolEnabled = false;
    public int mapPoolNumOfMaps = 0;
    public List<string> mapPoolMapList;
    
    public void Toggle()
    {
        this.mapPoolEnabled = !this.mapPoolEnabled;
    }
}