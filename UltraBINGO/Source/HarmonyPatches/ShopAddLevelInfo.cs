using System;
using HarmonyLib;
using TMPro;
using UltraBINGO.Util;
using UnityEngine;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(ShopZone), "TurnOn")]
public class ShopAddLevelInfo {
    [HarmonyPostfix]
    public static void AddBingoLevelInfo(ShopZone instance, Canvas shopCanvas) {
        if (!GameManager.IsInBingoLevel || shopCanvas == null ||
            shopCanvas.gameObject.name.Contains("Shop")) return;

        try {
            var origTip = instance.tipOfTheDay;
            var coords = $"{GameManager.CurrentRow}-{GameManager.CurrentColumn}";

            var teamClaim = GameManager.CurrentGame.Grid.LevelTable[coords].ClaimedBy;
            var time = GameManager.CurrentGame.Grid.LevelTable[coords].TimeToBeat;

            var secs = time;
            float mins = 0;
            while (secs >= 60f) {
                secs -= 60f;
                mins += 1f;
            }

            var formattedTime = $"{mins}:{secs:00.000}";

            const string unclaimed =
                "This level is currently <color=orange>unclaimed</color>.\nHurry and be the first to <color=green>claim it for your team</color>!";
            const string claimedByOwnTeam =
                "This level is currently <color=green>claimed by your team</color>.\n<color=orange>Choose another level</color> to claim, or <color=orange>try and improve the current requirement</color> to make it harder for other teams to reclaim!";
            const string claimedByOtherTeam =
                "This level is currently claimed by another team.\n<color=orange>Beat</color> the current requirement to <color=green>reclaim it for your team</color>!";

            if (teamClaim == "NONE")
                origTip.text = unclaimed;
            else
                origTip.text =
                    $"Claimed by: <color={teamClaim.ToLower()}>{teamClaim}</color> team\n\nTIME TO BEAT: <color=orange>{formattedTime}</color>\n\n{(teamClaim == GameManager.CurrentTeam ? claimedByOwnTeam : claimedByOtherTeam)}";

            //Hide the CG and sandbox buttons
            var shopObject =
                GetGameObjectChild(
                    GetGameObjectChild(GetGameObjectChild(shopCanvas.gameObject, "Background"), "Main Panel"),
                    "Main Menu");

            var cgButton = GetGameObjectChild(GetGameObjectChild(shopObject, "Buttons"), "CyberGrindButton");
            cgButton?.SetActive(false);

            var sandboxButton = GetGameObjectChild(GetGameObjectChild(shopObject, "Buttons"), "SandboxButton");
            sandboxButton?.SetActive(false);
        } catch (Exception) {
            Logging.Warn("This shop isn't vanilla or an error occured");
        }
    }
}