using HarmonyLib;
using TMPro;

// ReSharper disable InconsistentNaming

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch]
public static class DifficultyTitlePatches {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DifficultyTitle), nameof(DifficultyTitle.Check))]
    public static bool DisplayUltraBingoTitle(DifficultyTitle __instance, bool ___lines, ref TMP_Text ___txt2) {
        if (GameManager.IsInBingoLevel) {
            var text = (___lines ? "-- " : "") + "ULTRABINGO" + (___lines ? " --" : "");

            if (!___txt2) ___txt2 = __instance.GetComponent<TMP_Text>();
            if (___txt2) ___txt2.text = text;

            return false;
        } else {
            return true;
        }
    }
}