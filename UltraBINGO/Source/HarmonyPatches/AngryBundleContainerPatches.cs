using AngryLevelLoader.Containers;
using HarmonyLib;
using UltraBINGO.UI;
using UltraBINGO.Util;

// ReSharper disable InconsistentNaming

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch]
public static class AngryBundleContainerPatches {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AngryBundleContainer), nameof(AngryBundleContainer.UpdateFinalRankUI))]
    public static void NotifyDownloadComplete() {
        if (GameManager.currentSetGame == null || GameManager.EnteringAngryLevel) return;

        Logging.Info($"Download of {BingoMenuController.currentlyDownloadingLevel} finished");
        GameManager.IsDownloadingLevel = false;

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
            $"<color=orange>{BingoMenuController.currentlyDownloadingLevel}</color> has finished downloading.\nClick level button again to enter!"
        );

        BingoMenuController.currentlyDownloadingLevel = "";
    }
}