using AngryLevelLoader.Containers;
using AngryLevelLoader.Managers;
using HarmonyLib;
using RudeLevelScript;
using UltraBINGO.Util;

// ReSharper disable InconsistentNaming

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch]
public static class AngrySceneManagerPatches {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AngrySceneManager), nameof(AngrySceneManager.LevelButtonPressed))]
    public static bool PreventNormalLoadInBingoGame(
        AngryBundleContainer bundleContainer,
        LevelContainer levelContainer,
        RudeLevelData levelData,
        string levelName
    ) {
        if (!GameManager.IsInBingoLevel || GameManager.CurrentGame.IsGameFinished()) return true;

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
            "Cannot load Angry levels while in a bingo game."
        );
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AngrySceneManager), nameof(AngrySceneManager.LoadLevel))]
    public static bool PreventLoadLinkedLevel(
        AngryBundleContainer bundleContainer,
        LevelContainer levelContainer,
        RudeLevelData levelData,
        string levelPath,
        bool showBlocker = true
    ) {
        return !GameManager.IsInBingoLevel || CommonFunctions.GetSceneName() == "Main Menu" ||
               GameManager.EnteringAngryLevel;
    }
}