using System;
using HarmonyLib;
using TMPro;
using UltraBINGO.UI_Elements;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(OptionsMenuToManager),"QuitMission")]
public static class ConfirmLeaveGame
{
    [HarmonyPrefix]
    public static bool confirmLeaveInGame(ref OptionsMenuToManager __instance)
    {
        if(GameManager.isInBingoLevel)
        {
            TextMeshProUGUI leaveText = GetGameObjectChild(GetGameObjectChild(__instance.quitDialog.gameObject,"Panel"),"Text (2)").GetComponent<TextMeshProUGUI>();
            if(GameManager.playerIsHost())
            {
                leaveText.text = "<color=orange>WARNING</color>: Leaving now will <color=orange>end the game</color> for all players.\nAre you sure?";
            }
            else
            {
                leaveText.text = "Are you sure you want to leave?\n(<color=orange>WARNING</color>: You will not be able to rejoin.)";
            }
            __instance.quitDialog.ShowDialog();
            return false;
        }
        else
        {
            return true;
        }
    }
}

[HarmonyPatch(typeof(OptionsManager),"Start")]
public static class PauseMenu
{
    [HarmonyPostfix]
    public static void patchPauseMenu(ref OptionsManager __instance)
    {
        if(GameManager.isInBingoLevel && getSceneName() != "Main Menu")
        {
            GetGameObjectChild(GetGameObjectChild(__instance.pauseMenu,"Quit Mission"),"Text").GetComponent<TextMeshProUGUI>().text = "LEAVE GAME";
            BingoCardPauseMenu.Init(ref __instance);
            BingoCardPauseMenu.ShowBingoCardInPauseMenu(ref __instance);
        }
    }
    
}

[HarmonyPatch(typeof(OptionsMenuToManager),"QuitMissionNoConfirm")]
public static class QuitLevel
{
    [HarmonyPrefix]
    public static bool returnFromBingoLevel()
    {
        GameManager.isInBingoLevel = false;
        GameManager.returningFromBingoLevel = true;
        GameManager.hasSent = false;
        return true;
    }
}