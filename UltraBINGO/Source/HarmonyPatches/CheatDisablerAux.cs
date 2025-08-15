using HarmonyLib;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(CheatsManager), "Start")]
public class CheatDisablerAux {
    [HarmonyPrefix]
    public static bool DisableCheatFunctions() {
        if (!GameManager.IsInBingoLevel) return true;
        
        var cheatManager = MonoSingleton<CheatsManager>.Instance.gameObject;
        
        cheatManager.GetComponentInChildren<SandboxHud>().enabled = false;
        cheatManager.GetComponentInChildren<CheatsManager>().enabled = false;
        cheatManager.SetActive(false);
        
        return false;
    }
}