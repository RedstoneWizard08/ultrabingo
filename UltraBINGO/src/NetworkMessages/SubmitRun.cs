using System;

namespace UltraBINGO.NetworkMessages;

// Submit a run to the server.
public class SubmitRunRequest : SendMessage {
    public new string messageType = "SubmitRun";

    public string team;
    public int gameId;

    public int column;
    public int row;

    public string levelName;
    public string levelId;
    public string playerName;
    public string steamId;

    public float time;

    public RegisterTicket ticket;
}

// Notify all players that a map has been claimed/reclaimed/improved.
public class LevelClaimNotification : PlayerNotification {
    public int claimType; //0:Claimed , 1: Improved, 2: Reclaimed
    public string username;

    public string levelname;
    public string team;

    public int row;
    public int column;

    public float newTimeRequirement;

    public bool isMapVoted;
}

public static class LevelClaimHandler {
    public static void Handle(LevelClaimNotification response) {
        try {
            if (response == null) {
                Logging.Error("Level claim response was null!");
                throw new ArgumentNullException();
            }

            var actionType = response.claimType switch {
                0 => "claimed ",
                1 => "improved ",
                2 => "reclaimed ",
                _ => ""
            };

            var secs = response.newTimeRequirement;
            float mins = 0;
            while (secs >= 60f) {
                secs -= 60f;
                mins += 1f;
            }

            var formattedTime = mins + ":" + secs.ToString("00.000");
            var broadcastString = "<color=" + response.team.ToLower() + ">" + response.username +
                                  "</color> has <color=orange>" + actionType + response.levelname +
                                  "</color> for the <color=" + response.team.ToLower() + ">" + response.team +
                                  " </color>team (<color=orange>" + formattedTime + "</color>).";

            if (response.isMapVoted) broadcastString += "\n Cancelling reroll vote.";

            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(broadcastString);
            GameManager.UpdateCards(response.row, response.column, response.team, response.username,
                response.newTimeRequirement);
        } catch (Exception e) {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                "A level was claimed by someone but was unable to update the grid.\nCheck BepInEx console and report it to Clearwater!");
            Logging.Error(e.Message);
            throw;
        }
    }
}