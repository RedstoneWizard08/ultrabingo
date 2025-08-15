using HarmonyLib;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(FinalRank), "LevelChange")]
public static class LevelEndChanger {
    [HarmonyPrefix]
    public static bool HandleBingoLevelChange(FinalRank instance, float savedTime, bool force = false) {
        if (GameManager.IsInBingoLevel && !GameManager.CurrentGame.IsGameFinished()) {
            MonoSingleton<OptionsMenuToManager>.Instance.RestartMissionNoConfirm();
            return false;
        } else {
            return true;
        }
    }
}