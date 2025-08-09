using HarmonyLib;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(CheatsController), "ActivateCheats")]
public class CheatDisabler {
    [HarmonyPrefix]
    public static bool DisableCheatFunctions() {
        if (!GameManager.IsInBingoLevel) return true;

        var cheatManager = FindObjectWithInactiveRoot("Canvas", "Cheat Menu");

        if (cheatManager == null) return false;

        cheatManager.GetComponentInChildren<SandboxHud>().enabled = false;
        cheatManager.GetComponentInChildren<CheatsManager>().enabled = false;
        cheatManager.SetActive(false);

        return false;
    }
}