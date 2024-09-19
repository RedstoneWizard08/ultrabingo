using HarmonyLib;
using BepInEx;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(CanvasController),"Awake")]
public static class MainMenu
{
    [HarmonyPostfix]
    public static void addBingoButton(ref CanvasController __instance)
    {
        if(getSceneName() == "Main Menu")
        {
            UIManager.SetupElements(__instance);
        }
    }
    
}