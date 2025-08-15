using GameConsole.Commands;
using HarmonyLib;
using TMPro;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(DifficultyTitle), "Check")]
public static class LevelEndPanel {
    public static bool DisplayUltraBingoTitle(DifficultyTitle instance, ref TMP_Text txt2) {
        if (GameManager.IsInBingoLevel) {
            var text = "-- ULTRABINGO -- ";
            if (!txt2) txt2 = instance.GetComponent<TMP_Text>();
            if (txt2) txt2.text = text;

            return false;
        } else {
            return true;
        }
    }
}