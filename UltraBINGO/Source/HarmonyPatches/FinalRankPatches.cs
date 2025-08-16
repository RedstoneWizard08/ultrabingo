using System.Linq;
using HarmonyLib;
using UltraBINGO.Net;
using UltraBINGO.Packets;
using UltraBINGO.Util;

// ReSharper disable InconsistentNaming

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch]
public static class FinalRankPatches {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FinalRank), nameof(FinalRank.Update))]
    public static void SendResult(FinalRank __instance, float ___savedTime, int ___savedStyle) {
        if (!GameManager.IsInBingoLevel || GameManager.HasSent || GameManager.CurrentGame.IsGameFinished()) return;

        if (GameManager.CurrentGame.GameSettings.RequiresPRank) {
            var stats = MonoSingleton<StatsManager>.Instance;

            if (stats != null) {
                if (!(stats.seconds <= stats.timeRanks.Last() && stats.kills >= stats.killRanks.Last() &&
                      stats.stylePoints >= stats.styleRanks.Last() && stats.restarts == 0)) {
                    Logging.Message("P-Rank not obtained, rejecting run");

                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                        "You must finish the level with a <color=yellow>P</color>-Rank to claim it."
                    );

                    GameManager.HasSent = true;

                    return;
                }
            } else {
                Logging.Warn("Unable to get StatsManager?");
                return;
            }
        }

        Requests.SubmitRun(
            new SubmitRunRequest {
                PlayerName = CommonFunctions.SanitiseUsername(Steamworks.SteamClient.Name),
                SteamId = Steamworks.SteamClient.SteamId.ToString(),
                Team = GameManager.CurrentTeam,
                GameId = GameManager.CurrentGame.GameId,
                Time = ___savedTime,
                LevelName = CommonFunctions.GetSceneName(),
                LevelId = GameManager.CurrentGame.Grid
                    .LevelTable[$"{GameManager.CurrentRow}-{GameManager.CurrentColumn}"].LevelId,
                Column = GameManager.CurrentColumn,
                Row = GameManager.CurrentRow,
                Ticket = RegisterTicket.Create()
            }
        );

        GameManager.HasSent = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FinalRank), nameof(FinalRank.LevelChange))]
    public static bool HandleBingoLevelChange() {
        if (!GameManager.IsInBingoLevel || GameManager.CurrentGame.IsGameFinished()) return true;

        MonoSingleton<OptionsMenuToManager>.Instance.RestartMissionNoConfirm();

        return false;
    }
}