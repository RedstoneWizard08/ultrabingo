using AngryLevelLoader.Containers;
using AngryLevelLoader.Managers;
using HarmonyLib;
using RudeLevelScript;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(AngrySceneManager), "LoadLevel")]
public static class PreventAngryAutoloadLinkedLevel {
    [HarmonyPrefix]
    public static bool PreventLoadLinkedLevel(AngryBundleContainer bundleContainer, LevelContainer levelContainer,
        RudeLevelData levelData, string levelPath, bool showBlocker = true) {
        return !GameManager.IsInBingoLevel || CommonFunctions.GetSceneName() == "Main Menu" || GameManager.EnteringAngryLevel;
    }
}