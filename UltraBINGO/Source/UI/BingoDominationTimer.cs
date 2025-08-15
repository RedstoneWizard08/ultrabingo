using UnityEngine;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.UI;

public static class BingoDominationTimer {
    public static GameObject? Root;
    public static GameObject? Timer;

    public static void Init(ref GameObject dominationTimer) {
        Timer = GetGameObjectChild(dominationTimer, "Timer");
    }
}