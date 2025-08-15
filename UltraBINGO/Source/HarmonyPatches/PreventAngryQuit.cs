using HarmonyLib;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(OptionsManager), "QuitMission")]
public static class PreventAngryQuit {
    [HarmonyPrefix]
    public static bool PreventAngryQuitOverride() {
        return !GameManager.IsInBingoLevel;
    }
}