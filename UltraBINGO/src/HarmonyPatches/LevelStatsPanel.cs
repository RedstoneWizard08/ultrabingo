﻿using System;
using System.Threading.Tasks;
using AngryLevelLoader.Containers;
using AngryLevelLoader.Managers;
using HarmonyLib;
using RudeLevelScript;
using TMPro;
using UltraBINGO.Components;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UnityEngine;
using static UltraBINGO.CommonFunctions;
using Object = UnityEngine.Object;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(LevelStatsEnabler), "Start")]
public static class LevelStatsPanelPatchStart {
    [HarmonyPostfix]
    public static async void showBingoPanel(LevelStatsEnabler __instance) {
        //Give the game a sec to load in before displaying the tab panel.
        await Task.Delay(250);

        if (GameManager.IsInBingoLevel && GetSceneName() != "Main Menu") {
            __instance.gameObject.SetActive(true);

            var inGamePanel = Object.Instantiate(AssetLoader.BingoInGameGridPanel, __instance.gameObject.transform);
            inGamePanel.name = "BingoInGamePanel";

            var grid = GetGameObjectChild(inGamePanel, "Grid");

            var card = Object.Instantiate(BingoCardPauseMenu.Grid, grid.transform);
            card.name = "Card";

            switch (MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition", 0)) {
                case 2: // Right
                {
                    inGamePanel.transform.localPosition += new Vector3(-425f, 0f, 0f);
                    break;
                }
                default: // Left/middle
                {
                    inGamePanel.transform.localPosition += new Vector3(-10f, 0f, 0f);
                    break;
                }
            }

            switch (GameManager.CurrentGame.grid.size) {
                case 3: {
                    card.transform.localPosition = new Vector3(2.5f, -2.5f, 0f);
                    card.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                    break;
                }
                case 4: {
                    card.transform.localPosition = new Vector3(-2.5f, 0f, 0f);
                    card.transform.localScale = new Vector3(0.55f, 0.55f, 0.5f);
                    break;
                }
                case 5: {
                    card.transform.localPosition = new Vector3(-5f, 5f, 0f);
                    card.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
                    break;
                }

                default: {
                    break;
                }
            }

            BingoCardPauseMenu.inGamePanel = card;

            //Load the reroll panel.
            BingoVotePanel.Init(ref AssetLoader.BingoVotePanel);

            var votePanel = Object.Instantiate(AssetLoader.BingoVotePanel, __instance.gameObject.transform.parent);
            votePanel.AddComponent<BingoVoteManager>();
            votePanel.name = "VotePanel";
            votePanel.SetActive(false);

            MonoSingleton<BingoVoteManager>.Instance.CheckOngoingVote();

            //If playing domination, load the domination time remaining panel.
            //(Need to put the timeManager component in the root to ensure it remains active even while the panel is closed)
            if (GameManager.CurrentGame.gameSettings.gamemode == 1) {
                var canvas = GetInactiveRootObject("Canvas");
                var dominationTimeRemaining = Object.Instantiate(AssetLoader.BingoDominationTimer, canvas.transform);

                canvas.AddComponent<DominationTimeManager>();
                canvas.GetComponent<DominationTimeManager>().Bind(dominationTimeRemaining);
                dominationTimeRemaining.name = "BingoDominationTimer";
                dominationTimeRemaining.SetActive(true);
                dominationTimeRemaining.transform.localPosition -=
                    new Vector3(0f, 5f, 0f); //Align it better with other UI elements
            }
        }
    }
}

[HarmonyPatch(typeof(LevelStatsEnabler), "Update")]
public static class LevelStatsPanelPatchUpdate {
    [HarmonyPostfix]
    public static void showBingoPanel(ref LevelStatsEnabler __instance, LevelStats ___levelStats, bool ___keepOpen) {
        if (GameManager.IsInBingoLevel && GetSceneName() != "Main Menu") {
            var panel = GetGameObjectChild(__instance.gameObject, "BingoInGamePanel");

            if (GameManager.IsInBingoLevel && GetSceneName() != "Main Menu" && panel != null && ___levelStats != null)
                panel.SetActive(MonoSingleton<InputManager>.Instance.InputSource.Stats.IsPressed || ___keepOpen);
        }
    }
}