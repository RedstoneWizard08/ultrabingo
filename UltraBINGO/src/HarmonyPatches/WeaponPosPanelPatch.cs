using HarmonyLib;
using UnityEngine;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(ViewModelFlip), "OnPrefChanged")]
public class WeaponPosPanelPatch {
    [HarmonyPostfix]
    public static void ChangeBingoPanelPos(string key, object value) {
        if (key == "weaponHoldPosition" && GameManager.IsInBingoLevel && !GameManager.CurrentGame.IsGameFinished()) {
            var ctr = CommonFunctions.GetGameObjectChild(CommonFunctions.GetInactiveRootObject("Canvas"), "Level Stats Controller");
            var bingoPanel = CommonFunctions.GetGameObjectChild(ctr, "BingoInGamePanel");
            switch (MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition", 0)) {
                case 2: // Right
                {
                    bingoPanel.transform.localPosition = new Vector3(-425f, 0f, 0f);
                    break;
                }
                default: // Left/middle
                {
                    bingoPanel.transform.localPosition = new Vector3(300f, 0f, 0f);
                    break;
                }
            }
        }
    }
}