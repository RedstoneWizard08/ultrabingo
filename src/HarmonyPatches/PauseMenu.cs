using System;
using HarmonyLib;
using TMPro;
using UltraBINGO.UI_Elements;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(OptionsManager),"Start")]
public static class PauseMenu
{
    [HarmonyPostfix]
    public static void patchPauseMenu(ref OptionsManager __instance)
    {
        if(GameManager.isInBingoLevel && getSceneName() != "Main Menu")
        {
            GetGameObjectChild(GetGameObjectChild(__instance.pauseMenu,"Quit Mission"),"Text").GetComponent<TextMeshProUGUI>().text = "RETURN TO CARD";
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