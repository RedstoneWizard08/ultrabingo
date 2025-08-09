using HarmonyLib;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch]
public static class OptionsMenuToManagerPatches {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(OptionsMenuToManager), nameof(OptionsMenuToManager.QuitMissionNoConfirm))]
    public static bool LeaveBingoGame() {
        if (GameManager.IsInBingoLevel) GameManager.LeaveGame(true);
        
        return true;
    }
}