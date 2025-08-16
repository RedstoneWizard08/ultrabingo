using HarmonyLib;
using UltraBINGO.Util;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch]
public static class ViewModelFlipPatches {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ViewModelFlip), nameof(ViewModelFlip.OnPrefChanged))]
    public static void ChangeBingoPanelPos(string key, object value) {
        if (key != "weaponHoldPosition" || !GameManager.IsInBingoLevel ||
            GameManager.CurrentGame.IsGameFinished()) return;

        var ctr = CommonFunctions.FindObjectWithInactiveRoot(
            "Canvas",
            "Level Stats Controller"
        );

        var bingoPanel = CommonFunctions.GetGameObjectChild(ctr, "BingoInGamePanel");

        if (bingoPanel != null)
            bingoPanel.transform.localPosition =
                MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition") switch {
                    2 => // Right
                        new Vector3(-425f, 0f, 0f),
                    _ => new Vector3(300f, 0f, 0f)
                };
    }
}