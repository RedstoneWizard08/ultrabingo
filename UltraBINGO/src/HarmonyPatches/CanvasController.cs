using HarmonyLib;
using BepInEx;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(CanvasController), "Awake")]
public static class MainMenu {
    [HarmonyPostfix]
    public static void AddBingoButton(ref CanvasController __instance) {
        if (GetSceneName() == "Main Menu") UIManager.SetupElements(__instance);
    }
}