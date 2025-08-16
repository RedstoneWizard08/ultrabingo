using HarmonyLib;
using TMPro;
using UltraBINGO.Util;

// ReSharper disable InconsistentNaming

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch]
public static class OptionsMenuToManagerPatches {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(OptionsMenuToManager), nameof(OptionsMenuToManager.QuitMission))]
    public static bool ConfirmLeaveInGame(ref OptionsMenuToManager __instance) {
        if (GameManager.IsInBingoLevel) {
            var leaveText = CommonFunctions.FindObject(__instance.quitDialog.gameObject, "Panel", "Text (2)")
                ?.GetComponent<TextMeshProUGUI>();

            if (leaveText != null)
                leaveText.text = GameManager.PlayerIsHost()
                    ? "<color=orange>WARNING</color>: YOU ARE THE HOST. Leaving now will <color=orange>end the game</color> for all players.\nAre you sure?"
                    : "Leave game in progress?\n(<color=orange>WARNING</color>: You will not be able to rejoin.)";

            __instance.quitDialog.ShowDialog();

            return false;
        } else {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(OptionsMenuToManager), nameof(OptionsMenuToManager.QuitMissionNoConfirm))]
    public static bool LeaveBingoGame() {
        if (GameManager.IsInBingoLevel) GameManager.LeaveGame(true);

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(OptionsMenuToManager), nameof(OptionsMenuToManager.QuitMissionNoConfirm))]
    public static bool ReturnFromBingoLevel() {
        GameManager.IsInBingoLevel = false;
        GameManager.HasSent = false;
        return true;
    }
}