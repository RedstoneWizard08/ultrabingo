using System;
using AngryLevelLoader.Containers;
using AngryLevelLoader.Managers;
using HarmonyLib;
using RudeLevelScript;
using TMPro;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(OptionsMenuToManager),"QuitMissionNoConfirm")]
public static class LeaveBingoGame
{
    [HarmonyPrefix]
    public static bool leaveBingoGame()
    {
        if(GameManager.isInBingoLevel)
        {
            GameManager.LeaveGame(true);
        }
        return true;
    }
}

//When finishing an Angry level in bingo, make sure we don't go to the next linked level if there is one.
[HarmonyPatch(typeof(AngrySceneManager),"LoadLevel")]
public static class preventAngryAutoloadLinkedLevel
{
    [HarmonyPrefix]
    public static bool preventLoadLinkedLevel(AngryBundleContainer bundleContainer, LevelContainer levelContainer, RudeLevelData levelData, string levelPath, bool showBlocker = true)
    {
        if(GameManager.isInBingoLevel && getSceneName() != "Main Menu" && !GameManager.enteringAngryLevel)
        {
            return false;
        }
        return true;
    }
}

//When finishing an Angry level in bingo, make sure Angry doesn't override and quit to main menu.
[HarmonyPatch(typeof(OptionsManager),"QuitMission")]
public static class preventAngryQuit
{
    [HarmonyPrefix]
    public static bool preventAngryQuitOverride()
    {
        if(GameManager.isInBingoLevel)
        {
            return false;
        }
        return true;
    }
}

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
                leaveText.text = "Leave game in progress?\n(<color=orange>WARNING</color>: You will not be able to rejoin.)";
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