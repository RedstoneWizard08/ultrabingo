using HarmonyLib;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(CheatsController), "Start")]
public class RemoveCheatFlagInBingo {
    [HarmonyPostfix]
    public static void DisableCheatFlagInBingo() {
        if (GameManager.IsInBingoLevel) MonoSingleton<AssistController>.Instance.cheatsEnabled = false;
    }
}