using System;
using System.Threading.Tasks;
using AngryLevelLoader.Containers;
using AngryLevelLoader.Managers;
using HarmonyLib;
using RudeLevelScript;
using TMPro;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using UnityEngine;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(LevelStatsEnabler),"Start")]
public static class LevelStatsPanelPatchStart
{
    [HarmonyPostfix]
    public static async void showBingoPanel(LevelStatsEnabler __instance)
    {
        //Give the game a sec to load in before displaying the tab panel.
        await Task.Delay(250);
        
        if(GameManager.IsInBingoLevel && getSceneName() != "Main Menu")
        {
            GameObject inGamePanel = GameObject.Instantiate(AssetLoader.BingoInGameGridPanel,__instance.gameObject.transform);
            inGamePanel.name = "BingoInGamePanel";
            
            GameObject grid = GetGameObjectChild(inGamePanel,"Grid");
            switch(MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition", 0))
            {
                case 2: // Right
                {
                    inGamePanel.transform.localPosition += new Vector3(-425f,0f,0f);
                    break;
                }
                default: // Left/middle
                {
                    inGamePanel.transform.localPosition += new Vector3(-5f,0f,0f);
                    break;
                }
            }
            
            
            grid.transform.localPosition += new Vector3(-5f,5f,0f);
            
            GameObject card = GameObject.Instantiate(BingoCardPauseMenu.Grid,grid.transform);
            card.name = "Card";
            card.transform.localScale = new Vector3(0.45f,0.45f,0.45f);
            card.transform.localPosition = Vector3.zero;    
            
            BingoCardPauseMenu.inGamePanel = card;
        }
    }
}

[HarmonyPatch(typeof(LevelStatsEnabler),"Update")]
public static class LevelStatsPanelPatchUpdate
{
    [HarmonyPostfix]
    public static void showBingoPanel(ref LevelStatsEnabler __instance, LevelStats ___levelStats, bool ___keepOpen)
    {
        if(GameManager.IsInBingoLevel && getSceneName() != "Main Menu")
        {
            GameObject panel = GetGameObjectChild(__instance.gameObject,"BingoInGamePanel");
        
            if(GameManager.IsInBingoLevel && getSceneName() != "Main Menu" && panel != null && ___levelStats != null)
            {
                panel.SetActive(MonoSingleton<InputManager>.Instance.InputSource.Stats.IsPressed || ___keepOpen);
            }
        }
    }
}