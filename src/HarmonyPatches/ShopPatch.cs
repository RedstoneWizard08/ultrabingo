using System;
using HarmonyLib;
using TMPro;
using UltrakillBingoClient;
using UnityEngine;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(ShopZone),"TurnOn")]
public class ShopAddLevelInfo
{
    [HarmonyPostfix]
    public static void AddBingoLevelInfo(ShopZone __instance, Canvas ___shopCanvas)
    {
        if(GameManager.IsInBingoLevel)
        {
            if(___shopCanvas != null && !___shopCanvas.gameObject.name.Contains("Shop"))
            {
                TextMeshProUGUI origTip = GetTextMeshProGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(___shopCanvas.gameObject,"TipBox"),"Panel"),"TipText"));
                
                string coords = GameManager.CurrentRow + "-" + GameManager.CurrentColumn;
                
                string teamClaim = GameManager.CurrentGame.grid.levelTable[coords].claimedBy;
                float time = GameManager.CurrentGame.grid.levelTable[coords].timeToBeat;
                int style = GameManager.CurrentGame.grid.levelTable[coords].styleToBeat;
                
                float secs = time;
                float mins = 0;
                while (secs >= 60f)
                {
                    secs -= 60f;
                    mins += 1f;
                }
                string formattedTime = mins + ":" + secs.ToString("00.000");
                
                string unclaimed = "This level is currently <color=orange>unclaimed</color>.\nHurry and be the first to <color=green>claim it for your team</color>!";
                string claimedByOwnTeam = "This level is currently <color=green>claimed by your team</color>.\n<color=orange>Choose another level</color> to claim, or <color=orange>try and improve the current requirement</color> to make it harder for other teams to reclaim!";
                string claimedByOtherTeam = "This level is currently claimed by the OTHER team.\n<color=orange>Beat</color> the current requirement to <color=green>reclaim it for your team</color>!";
                
                if(teamClaim == "NONE")
                {
                    origTip.text = unclaimed;
                }
                else
                {
                    origTip.text = "Claimed by: " + teamClaim + "\n\n" +
                    (GameManager.CurrentGame.gameSettings.gameType == 0 ? "TIME" : "STYLE") + " TO BEAT: <color=orange>" + (GameManager.CurrentGame.gameSettings.gameType == 0 ? formattedTime : style) + "</color>\n\n" +
                    (teamClaim == GameManager.CurrentTeam ? claimedByOwnTeam : claimedByOtherTeam);
                }
                
                //Hide the CG and sandbox buttons
                GameObject cgButton = GetGameObjectChild(GetGameObjectChild(___shopCanvas.gameObject,"Main Menu"),"CyberGrindButton");
                cgButton.SetActive(false);
                
                GameObject sandboxButton = GetGameObjectChild(GetGameObjectChild(___shopCanvas.gameObject,"Main Menu"),"SandboxButton");
                sandboxButton.SetActive(false);
            }
        }
    }
}