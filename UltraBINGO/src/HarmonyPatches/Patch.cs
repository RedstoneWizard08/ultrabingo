using System;
using AngryLevelLoader.Containers;
using HarmonyLib;
using UltraBINGO.UI;
using UltraBINGO.Util;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(AngryBundleContainer), "UpdateFinalRankUI")]
public static class Patch {
    [HarmonyPostfix]
    public static void NotifyDownloadComplete(AngryBundleContainer instance) {
        if (GameManager.currentSetGame == null || GameManager.EnteringAngryLevel) return;
        
        Logging.Info($"Download of {BingoMenuController.currentlyDownloadingLevel} finished");
        GameManager.IsDownloadingLevel = false;

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
            $"<color=orange>{BingoMenuController.currentlyDownloadingLevel}</color> has finished downloading.\nClick level button again to enter!");

        BingoMenuController.currentlyDownloadingLevel = "";
    }
}