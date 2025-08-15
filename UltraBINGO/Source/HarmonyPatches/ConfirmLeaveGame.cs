using HarmonyLib;
using TMPro;
using UltraBINGO.Util;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(OptionsMenuToManager), "QuitMission")]
public static class ConfirmLeaveGame {
    [HarmonyPrefix]
    public static bool ConfirmLeaveInGame(ref OptionsMenuToManager instance) {
        if (GameManager.IsInBingoLevel) {
            var leaveText = CommonFunctions.FindObject(instance.quitDialog.gameObject, "Panel", "Text (2)")
                ?.GetComponent<TextMeshProUGUI>();

            if (leaveText != null)
                leaveText.text = GameManager.PlayerIsHost()
                    ? "<color=orange>WARNING</color>: YOU ARE THE HOST. Leaving now will <color=orange>end the game</color> for all players.\nAre you sure?"
                    : "Leave game in progress?\n(<color=orange>WARNING</color>: You will not be able to rejoin.)";

            instance.quitDialog.ShowDialog();

            return false;
        } else {
            return true;
        }
    }
}