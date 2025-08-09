using System.Linq;
using HarmonyLib;
using UltraBINGO.Net;
using UltraBINGO.Packets;
using UltraBINGO.Util;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(FinalRank), "Update")]
public class FinalRankFanfare {
    [HarmonyPostfix]
    public static void SendResult(FinalRank instance, float savedTime, int savedStyle) {
        if (GameManager.IsInBingoLevel && !GameManager.HasSent && !GameManager.CurrentGame.IsGameFinished()) {
            if (GameManager.CurrentGame.GameSettings.RequiresPRank) {
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

            var time = savedTime;

            var srr = new SubmitRunRequest {
                PlayerName = CommonFunctions.SanitiseUsername(Steamworks.SteamClient.Name),
                SteamId = Steamworks.SteamClient.SteamId.ToString(),
                Team = GameManager.CurrentTeam,
                GameId = GameManager.CurrentGame.GameId,
                Time = time,
                LevelName = CommonFunctions.GetSceneName(),
                LevelId = GameManager.CurrentGame.Grid
                    .LevelTable[$"{GameManager.CurrentRow}-{GameManager.CurrentColumn}"].LevelId,
                Column = GameManager.CurrentColumn,
                Row = GameManager.CurrentRow,
                Ticket = NetworkManager.CreateRegisterTicket()
            };

            NetworkManager.SubmitRun(srr);
            GameManager.HasSent = true;
        }
    }
}