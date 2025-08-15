using System.Threading.Tasks;
using HarmonyLib;
using TMPro;
using UltraBINGO.UI;
using UltraBINGO.Util;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

//When finishing an Angry level in bingo, make sure we don't go to the next linked level if there is one.

//When finishing an Angry level in bingo, make sure Angry doesn't override and quit to main menu.

[HarmonyPatch(typeof(OptionsManager), "Awake")]
public static class PauseMenu {
    [HarmonyPostfix]
    public static void PatchPauseMenu(OptionsManager instance) {
        Task.Delay(200).Wait();

        if (!GameManager.IsInBingoLevel || GetSceneName() == "Main Menu") return;

        var canvas = GetInactiveRootObject("Canvas");
        var pauseMenu = FindObject(canvas, "PauseMenu");

        if (pauseMenu == null) {
            Logging.Warn("Pause menu is null, waiting and trying again");
            pauseMenu = instance.pauseMenu;
        }


        FindObject(pauseMenu, "Quit Mission", "Text").GetComponent<TextMeshProUGUI>().text = "LEAVE GAME";

        BingoCardPauseMenu.Init(ref instance);
        BingoCardPauseMenu.ShowBingoCardInPauseMenu(ref instance);
    }
}