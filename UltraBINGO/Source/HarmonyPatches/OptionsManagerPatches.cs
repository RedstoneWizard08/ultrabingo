using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using TMPro;
using UltraBINGO.UI;
using UltraBINGO.Util;
using static UltraBINGO.Util.CommonFunctions;

// ReSharper disable InconsistentNaming

namespace UltraBINGO.HarmonyPatches;

//When finishing an Angry level in bingo, make sure we don't go to the next linked level if there is one.
//When finishing an Angry level in bingo, make sure Angry doesn't override and quit to main menu.

[HarmonyPatch]
public static class OptionsManagerPatches {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(OptionsManager), nameof(OptionsManager.Awake))]
    public static void PatchPauseMenu(OptionsManager __instance) {
        Thread.Sleep(200);

        if (!GameManager.IsInBingoLevel || GetSceneName() == "Main Menu") return;

        var canvas = GetInactiveRootObject("Canvas");
        var pauseMenu = FindObject(canvas, "PauseMenu");

        if (pauseMenu == null) {
            Logging.Warn("Pause menu is null, waiting and trying again");
            pauseMenu = __instance.pauseMenu;
        }


        FindObject(pauseMenu, "Quit Mission", "Text")?.GetComponent<TextMeshProUGUI>().SetText("LEAVE GAME");

        BingoCardPauseMenu.Init(ref __instance);
        BingoCardPauseMenu.ShowBingoCardInPauseMenu();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(OptionsManager), nameof(OptionsManager.QuitMission))]
    public static bool PreventAngryQuitOverride() {
        return !GameManager.IsInBingoLevel;
    }
}