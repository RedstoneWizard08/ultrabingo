using HarmonyLib;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(LevelStatsEnabler), "Update")]
public static class LevelStatsPanelPatchUpdate {
    [HarmonyPostfix]
    public static void ShowBingoPanel(ref LevelStatsEnabler instance, LevelStats levelStats, bool keepOpen) {
        if (GameManager.IsInBingoLevel && CommonFunctions.GetSceneName() != "Main Menu") {
            var panel = CommonFunctions.GetGameObjectChild(instance.gameObject, "BingoInGamePanel");

            if (GameManager.IsInBingoLevel && CommonFunctions.GetSceneName() != "Main Menu" && panel != null && levelStats != null)
                panel.SetActive(MonoSingleton<InputManager>.Instance.InputSource.Stats.IsPressed || keepOpen);
        }
    }
}