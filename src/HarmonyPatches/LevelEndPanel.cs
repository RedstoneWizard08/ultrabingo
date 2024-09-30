using GameConsole.Commands;
using HarmonyLib;
using TMPro;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(DifficultyTitle),"Check")]
public static class LevelEndPanel
{
    public static bool displayUltraBingoTitle(DifficultyTitle __instance, ref TMP_Text ___txt2)
    {
        if(GameManager.isInBingoLevel)
        {
            string text = "-- ULTRABINGO -- ";
            if(!___txt2)
            {
                ___txt2 = __instance.GetComponent<TMP_Text>();
            }
            if(___txt2)
            {
                ___txt2.text = text;
            }
            
            return false;
        }
        else
        {
            return true;
        }
    }
}

[HarmonyPatch(typeof(FinalRank),"LevelChange")]
public static class LevelEndChanger
{
    [HarmonyPrefix]
    public static bool handleBingoLevelChange(FinalRank __instance, float ___savedTime, int ___savedStyle, bool force = false)
    {
        if(GameManager.isInBingoLevel)
        {
            MonoSingleton<OptionsMenuToManager>.Instance.RestartMissionNoConfirm();
            return false;
        }
        else
        {
            return true;
        }
    }
}


[HarmonyPatch(typeof(FinalRank),"Update")]
public class FinalRankFanfare
{
    [HarmonyPostfix]
    public static void sendResult(FinalRank __instance, float ___savedTime, int ___savedStyle)
    {
        if(GameManager.isInBingoLevel && !GameManager.hasSent)
        {
            float time = ___savedTime;
            int style = ___savedStyle;

            SubmitRunRequest srr = new SubmitRunRequest();
            
            srr.playerName = Steamworks.SteamClient.Name;
            srr.steamId = Steamworks.SteamClient.SteamId.ToString();
            srr.team = GameManager.currentTeam;
            
            srr.gameId = GameManager.CurrentGame.gameId;
            srr.time = time;
            srr.style = style;
            srr.mapName = getSceneName();
            srr.column = GameManager.currentColumn;
            srr.row = GameManager.currentRow;

            
            NetworkManager.SubmitRun(srr);
            
            GameManager.hasSent = true;
            
        }
    }
}