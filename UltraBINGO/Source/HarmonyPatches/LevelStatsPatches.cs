using HarmonyLib;
using TMPro;
using UnityEngine;
using static UltraBINGO.Util.CommonFunctions;

// ReSharper disable InconsistentNaming

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch]
public static class LevelStatsPatches {
    private static GameObject? _originalChallengeText;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LevelStats), nameof(LevelStats.Start))]
    public static void SetupStatWindow(ref LevelStats __instance) {
        if (!GameManager.IsInBingoLevel) return;

        if (GetSceneName().Contains("P-") || GetSceneName().Contains("-E")) {
            // Prime or Encore level
            var majorAssists = __instance.majorAssists.transform.parent.gameObject;

            majorAssists.GetComponent<TextMeshProUGUI>().text = "TO BEAT:";

            _originalChallengeText = GetGameObjectChild(__instance.gameObject, "Challenge Title");
            _originalChallengeText?.GetComponent<TextMeshProUGUI>().SetText("CLAIMED BY:");
            _originalChallengeText?.SetActive(true);

            __instance.GetComponent<RectTransform>().sizeDelta = new Vector2(285f, 285f);
        } else {
            // Normal level
            var secrets = __instance.secrets[0].transform.parent.gameObject;
            secrets.SetActive(false);

            var challenge = __instance.challenge.transform.parent.gameObject;
            challenge.GetComponent<TextMeshProUGUI>().text = "TO BEAT:";

            var majorAssists = __instance.majorAssists.transform.parent.gameObject;
            majorAssists.GetComponent<TextMeshProUGUI>().text = "CLAIMED BY:";
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LevelStats), nameof(LevelStats.CheckStats))]
    public static void ShowRequirements(ref LevelStats __instance) {
        if (!GameManager.IsInBingoLevel) return;

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
            ? $"<color={GameManager.CurrentGame.Grid.LevelTable[coords].ClaimedBy?.ToLower()}>{currentTeamClaim}</color>"
            : "NONE";

        // If we're in a Prime or Encore level, use major assists text for to beat, and challenge text for claimed by.
        if (GetSceneName().Contains("P-") || GetSceneName().Contains("-E")) {
            __instance.majorAssists.GetComponent<TextMeshProUGUI>().text = formattedTime;

            var challenge = FindObject(__instance.gameObject, "Challenge Title", "Challenge")
                ?.GetComponent<TextMeshProUGUI>();

            if (challenge == null) return;

            challenge.text = colorTag;
            challenge.fontSize = 20;
        } else {
            __instance.challenge.GetComponent<TextMeshProUGUI>().text = formattedTime;

            __instance.majorAssists.GetComponent<TextMeshProUGUI>().text = colorTag;
            __instance.majorAssists.GetComponent<TextMeshProUGUI>().fontSize = 20;
        }
    }
}