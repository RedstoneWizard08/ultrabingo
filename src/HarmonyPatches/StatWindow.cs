using System;
using HarmonyLib;
using TMPro;
using UltrakillBingoClient;
using UnityEngine;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(LevelStats),"Start")]
public class StatWindowStart
{
    public static GameObject originalChallengeText;
    
    [HarmonyPostfix]
    public static void setupStatWindow(ref LevelStats __instance)
    {
        if(GameManager.IsInBingoLevel)
        {
            
            GameObject secrets = __instance.secrets[0].transform.parent.gameObject;
            secrets.SetActive(false);
            
            if(getSceneName().Contains("P-"))
            {
                GameObject majorAssists = __instance.majorAssists.transform.parent.gameObject;
                majorAssists.GetComponent<TextMeshProUGUI>().text = "TO BEAT:";
                
                originalChallengeText = GetGameObjectChild(__instance.gameObject,"Challenge Title");
                originalChallengeText.GetComponent<TextMeshProUGUI>().text = "CLAIMED BY:";
                originalChallengeText.SetActive(true);
                
                __instance.GetComponent<RectTransform>().sizeDelta = new Vector2(285f,285f);
            }
            else
            {
                GameObject challenge = __instance.challenge.transform.parent.gameObject;
                challenge.GetComponent<TextMeshProUGUI>().text = "TO BEAT:";
            
                GameObject majorAssists = __instance.majorAssists.transform.parent.gameObject;
                majorAssists.GetComponent<TextMeshProUGUI>().text = "CLAIMED BY:";
            }
            

        }
    }
}

[HarmonyPatch(typeof(LevelStats),"CheckStats")]
public class StatWindow
{
    public static GameObject originalChallengeText;
    
    [HarmonyPostfix]
    public static void showRequirements(ref LevelStats __instance)
    {
        if(GameManager.IsInBingoLevel)
        {
            string coords = GameManager.CurrentRow + "-" + GameManager.CurrentColumn;
            string currentTeamClaim = GameManager.CurrentGame.grid.levelTable[coords].claimedBy;
            
            float secs = GameManager.CurrentGame.grid.levelTable[coords].timeToBeat;
            float mins = 0;
            while (secs >= 60f)
            {
                secs -= 60f;
                mins += 1f;
            }
            
            string formattedTime = mins + ":" + secs.ToString("00.000");
            if(formattedTime == "0:00.000")
            {
                formattedTime = "<size=14>FINISH TO CLAIM" + (GameManager.CurrentGame.gameSettings.requiresPRank ? "(<color=#ffa200d9>P</color>)" : "") + "</size>";
            }
                        
            string colorTag = (currentTeamClaim != "NONE"
                ? ("<color="+GameManager.CurrentGame.grid.levelTable[coords].claimedBy.ToLower()+">" + currentTeamClaim + "</color>")  : "NONE");
            
            //If we're in a prime level, use major assists text for to beat, and challenge text for claimed by.
            if(getSceneName().Contains("P-"))
            {
                __instance.majorAssists.GetComponent<TextMeshProUGUI>().text = (GameManager.CurrentGame.gameSettings.gameType == 0 ?  (formattedTime) : GameManager.CurrentGame.grid.levelTable[coords].styleToBeat.ToString());

                GetGameObjectChild(GetGameObjectChild(__instance.gameObject,"Challenge Title"),"Challenge").GetComponent<TextMeshProUGUI>().text = colorTag;
                GetGameObjectChild(GetGameObjectChild(__instance.gameObject,"Challenge Title"),"Challenge").GetComponent<TextMeshProUGUI>().fontSize = 20;
                
            }
            else
            {
                __instance.challenge.GetComponent<TextMeshProUGUI>().text = (GameManager.CurrentGame.gameSettings.gameType == 0 ?  (formattedTime) : GameManager.CurrentGame.grid.levelTable[coords].styleToBeat.ToString());
                
                __instance.majorAssists.GetComponent<TextMeshProUGUI>().text = colorTag;
                __instance.majorAssists.GetComponent<TextMeshProUGUI>().fontSize = 20;
            }


          
        }
    }
}