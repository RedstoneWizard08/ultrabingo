using System;
using System.Threading.Tasks;
using AngryLevelLoader.Containers;
using AngryLevelLoader.Managers;
using HarmonyLib;
using RudeLevelScript;
using TMPro;
using UltraBINGO.Components;
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
            
            //Load the reroll panel.
            BingoVotePanel.Init(ref AssetLoader.BingoVotePanel);
            
            GameObject votePanel = GameObject.Instantiate(AssetLoader.BingoVotePanel,__instance.gameObject.transform.parent);
            votePanel.AddComponent<BingoVoteManager>();
            votePanel.name = "VotePanel";
            votePanel.SetActive(false);
            
            //If playing domination, load the domination time remaining panel.
            //(Need to put the timeManager component in the root to ensure it remains active even while the panel is closed)
            if(GameManager.CurrentGame.gameSettings.gamemode == 1)
            {
                GameObject dominationTimeRemaining = GameObject.Instantiate(AssetLoader.BingoDominationTimer,inGamePanel.gameObject.transform);
                
                GameObject canvas = GetInactiveRootObject("Canvas");
                
                canvas.AddComponent<DominationTimeManager>();
                canvas.GetComponent<DominationTimeManager>().Bind(dominationTimeRemaining);
                dominationTimeRemaining.name = "BingoDominationTimer";
                dominationTimeRemaining.SetActive(true);
                dominationTimeRemaining.transform.localPosition = new Vector3(350f,-25f,0f);
            }
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