using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(LevelStats), "CheckStats")]
public class StatWindow {
    public static GameObject originalChallengeText;

    [HarmonyPostfix]
    public static void ShowRequirements(ref LevelStats instance) {
        if (GameManager.IsInBingoLevel) {
            var coords = $"{GameManager.CurrentRow}-{GameManager.CurrentColumn}";
            var currentTeamClaim = GameManager.CurrentGame.Grid.LevelTable[coords].ClaimedBy;

            var secs = GameManager.CurrentGame.Grid.LevelTable[coords].TimeToBeat;
            float mins = 0;
            while (secs >= 60f) {
                secs -= 60f;
                mins += 1f;
            }

            var formattedTime = $"{mins}:{secs:00.000}";
            if (formattedTime == "0:00.000")
                formattedTime = $"<size=14>FINISH TO CLAIM{(GameManager.CurrentGame.GameSettings.RequiresPRank
                    ? "(<color=#ffa200d9>P</color>)"
                    : "")}</size>";

            var colorTag = currentTeamClaim != "NONE"
                ? $"<color={GameManager.CurrentGame.Grid.LevelTable[coords].ClaimedBy.ToLower()}>{currentTeamClaim}</color>"
                : "NONE";

            //If we're in a Prime or Encore level, use major assists text for to beat, and challenge text for claimed by.
            if (GetSceneName().Contains("P-") || GetSceneName().Contains("-E")) {
                instance.majorAssists.GetComponent<TextMeshProUGUI>().text = formattedTime;

                GetGameObjectChild(GetGameObjectChild(instance.gameObject, "Challenge Title"), "Challenge")
                    .GetComponent<TextMeshProUGUI>().text = colorTag;
                GetGameObjectChild(GetGameObjectChild(instance.gameObject, "Challenge Title"), "Challenge")
                    .GetComponent<TextMeshProUGUI>().fontSize = 20;
            } else {
                instance.challenge.GetComponent<TextMeshProUGUI>().text = formattedTime;

                instance.majorAssists.GetComponent<TextMeshProUGUI>().text = colorTag;
                instance.majorAssists.GetComponent<TextMeshProUGUI>().fontSize = 20;
            }
        }
    }
}