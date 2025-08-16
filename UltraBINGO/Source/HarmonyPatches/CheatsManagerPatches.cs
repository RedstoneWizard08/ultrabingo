using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch]
public static class CheatsManagerPatches {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CheatsManager), nameof(CheatsManager.Start))]
    public static bool DisableCheatFunctions() {
        if (!GameManager.IsInBingoLevel) return true;
        
        var cheatManager = MonoSingleton<CheatsManager>.Instance.gameObject;
        
        cheatManager.GetComponentInChildren<SandboxHud>().enabled = false;
        cheatManager.GetComponentInChildren<CheatsManager>().enabled = false;
        cheatManager.SetActive(false);
        
        return false;
    }
}