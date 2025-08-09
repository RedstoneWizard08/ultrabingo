using HarmonyLib;
using BepInEx;
using UltraBINGO.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(CanvasController), "Awake")]
public static class MainMenu {
    [HarmonyPostfix]
    public static void AddBingoButton(ref CanvasController instance) {
        if (GetSceneName() == "Main Menu") UIManager.SetupElements(instance);
    }
}