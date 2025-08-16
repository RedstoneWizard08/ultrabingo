using HarmonyLib;
using UltraBINGO.UI;
using static UltraBINGO.Util.CommonFunctions;

// ReSharper disable InconsistentNaming

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch]
public static class CanvasControllerPatches {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CanvasController), nameof(CanvasController.Awake))]
    public static void AddBingoButton(ref CanvasController __instance) {
        if (GetSceneName() == "Main Menu") UIManager.SetupElements(__instance);
    }
}