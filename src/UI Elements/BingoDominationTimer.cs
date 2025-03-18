using UnityEngine;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoDominationTimer
{
    public static GameObject Root;
    public static GameObject Timer;
    
    public static void Init(ref GameObject DominationTimer)
    {
        Timer = GetGameObjectChild(DominationTimer,"Timer");
    }
}