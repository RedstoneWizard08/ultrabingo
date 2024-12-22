using System;
using AngryLevelLoader.Containers;
using AngryLevelLoader.Managers;
using HarmonyLib;
using RudeLevelScript;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(AngrySceneManager),"LevelButtonPressed")]
public static class AngryStopNormalLoadPatch
{
    [HarmonyPrefix]
    public static bool preventNormalLoadInBingoGame(AngryBundleContainer bundleContainer, LevelContainer levelContainer, RudeLevelData levelData, string levelName)
    {
        if(GameManager.IsInBingoLevel && !GameManager.CurrentGame.isGameFinished())
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Cannot load Angry levels while in a bingo game.");
            return false;
        }
        return true;
    }
}