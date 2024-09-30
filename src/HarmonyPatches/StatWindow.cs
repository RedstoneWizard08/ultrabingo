using System;
using HarmonyLib;
using TMPro;
using UltrakillBingoClient;
using UnityEngine;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(LevelStats),"Start")]
public class StatWindowStart
{
    [HarmonyPostfix]
    public static void setupStatWindow(ref LevelStats __instance)
    {
        if(GameManager.isInBingoLevel)
        {
            Logging.Message("In bingo level, swapping level stats panel content");
            GameObject currentRequirementObject = __instance.time.transform.parent.gameObject;
            currentRequirementObject.GetComponent<TextMeshProUGUI>().text = (GameManager.CurrentGame.gameSettings.gameType == 0 ? "TIME:" : "STYLE:");
            
            GameObject secrets = __instance.secrets[0].transform.parent.gameObject;
            secrets.SetActive(false);
            
            GameObject challenge = __instance.challenge.transform.parent.gameObject;
            challenge.GetComponent<TextMeshProUGUI>().text = "TO BEAT:";
            
            GameObject majorAssists = __instance.majorAssists.transform.parent.gameObject;
            majorAssists.GetComponent<TextMeshProUGUI>().text = "CLAIMED BY:";
        }
    }
}

[HarmonyPatch(typeof(LevelStats),"CheckStats")]
public class StatWindow
{
    [HarmonyPostfix]
    public static void showRequirements(ref LevelStats __instance)
    {
        if(GameManager.isInBingoLevel)
        {
            string coords = GameManager.currentRow + "-" + GameManager.currentColumn;
            string currentTeamClaim = GameManager.CurrentGame.grid.levelTable[coords].claimedBy;
            
            float secs = GameManager.CurrentGame.grid.levelTable[coords].timeToBeat;
            float mins = 0;
            while (secs >= 60f)
            {
                secs -= 60f;
                mins += 1f;
            }
            
            string formattedTime = mins + ":" + secs.ToString("00.000");
            
            __instance.challenge.GetComponent<TextMeshProUGUI>().text = (GameManager.CurrentGame.gameSettings.gameType == 0 ?  (formattedTime) : GameManager.CurrentGame.grid.levelTable[coords].styleToBeat.ToString());
            
            string colorTag = (currentTeamClaim != "NONE"
                ? ("<color="+GameManager.CurrentGame.grid.levelTable[coords].claimedBy.ToLower()+">" + currentTeamClaim + "</color>")  : "NONE"); 
            
            __instance.majorAssists.GetComponent<TextMeshProUGUI>().text = colorTag;
            __instance.majorAssists.GetComponent<TextMeshProUGUI>().fontSize = 20;
          
        }
    }
}