using HarmonyLib;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(OptionsMenuToManager), "QuitMissionNoConfirm")]
public static class QuitLevel {
    [HarmonyPrefix]
    public static bool ReturnFromBingoLevel() {
        GameManager.IsInBingoLevel = false;
        GameManager.HasSent = false;
        return true;
    }
}