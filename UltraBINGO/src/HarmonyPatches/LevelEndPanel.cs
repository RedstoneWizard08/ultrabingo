using System.Linq;
using GameConsole.Commands;
using HarmonyLib;
using TMPro;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(DifficultyTitle), "Check")]
public static class LevelEndPanel {
    public static bool displayUltraBingoTitle(DifficultyTitle __instance, ref TMP_Text ___txt2) {
        if (GameManager.IsInBingoLevel) {
            var text = "-- ULTRABINGO -- ";
            if (!___txt2) ___txt2 = __instance.GetComponent<TMP_Text>();
            if (___txt2) ___txt2.text = text;

            return false;
        } else {
            return true;
        }
    }
}

[HarmonyPatch(typeof(FinalRank), "LevelChange")]
public static class LevelEndChanger {
    [HarmonyPrefix]
    public static bool handleBingoLevelChange(FinalRank __instance, float ___savedTime, bool force = false) {
        if (GameManager.IsInBingoLevel && !GameManager.CurrentGame.isGameFinished()) {
            MonoSingleton<OptionsMenuToManager>.Instance.RestartMissionNoConfirm();
            return false;
        } else {
            return true;
        }
    }
}

[HarmonyPatch(typeof(FinalRank), "Update")]
public class FinalRankFanfare {
    [HarmonyPostfix]
    public static void sendResult(FinalRank __instance, float ___savedTime, int ___savedStyle) {
        if (GameManager.IsInBingoLevel && !GameManager.HasSent && !GameManager.CurrentGame.isGameFinished()) {
            if (GameManager.CurrentGame.gameSettings.requiresPRank) {
                var sman = MonoSingleton<StatsManager>.Instance;
                if (sman != null) {
                    if (!(sman.seconds <= sman.timeRanks.Last() && sman.kills >= sman.killRanks.Last() &&
                          sman.stylePoints >= sman.styleRanks.Last() && sman.restarts == 0)) {
                        Logging.Message("P-Rank not obtained, rejecting run");
                        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                            "You must finish the level with a <color=yellow>P</color>-Rank to claim it.");
                        GameManager.HasSent = true;
                        return;
                    }
                } else {
                    Logging.Warn("Unable to get StatsManager?");
                }
            }

            var time = ___savedTime;

            var srr = new SubmitRunRequest {
                playerName = SanitiseUsername(Steamworks.SteamClient.Name),
                steamId = Steamworks.SteamClient.SteamId.ToString(),
                team = GameManager.CurrentTeam,
                gameId = GameManager.CurrentGame.gameId,
                time = time,
                levelName = GetSceneName(),
                levelId = GameManager.CurrentGame.grid
                    .levelTable[GameManager.CurrentRow + "-" + GameManager.CurrentColumn].levelId,
                column = GameManager.CurrentColumn,
                row = GameManager.CurrentRow,
                ticket = NetworkManager.CreateRegisterTicket()
            };

            NetworkManager.SubmitRun(srr);
            GameManager.HasSent = true;
        }
    }
}