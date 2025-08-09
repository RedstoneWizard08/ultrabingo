using System;
using AngryLevelLoader.Containers;
using AngryLevelLoader.Managers;
using HarmonyLib;
using RudeLevelScript;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(AngrySceneManager), "LevelButtonPressed")]
public static class AngryStopNormalLoadPatch {
    [HarmonyPrefix]
    public static bool PreventNormalLoadInBingoGame(AngryBundleContainer bundleContainer, LevelContainer levelContainer,
        RudeLevelData levelData, string levelName) {
        if (!GameManager.IsInBingoLevel || GameManager.CurrentGame.IsGameFinished()) return true;

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
            "Cannot load Angry levels while in a bingo game.");
        return false;
    }
}