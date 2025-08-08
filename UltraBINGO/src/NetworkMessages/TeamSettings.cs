﻿using System.Collections.Generic;

namespace UltraBINGO.NetworkMessages;

public class TeamSettings : SendMessage {
    public string messageType = "UpdateTeamSettings";
    public int gameId;
    public Dictionary<string, int> teams;
    public RegisterTicket ticket;
}

public class UpdateTeamsNotification : MessageResponse {
    public int status;
}

public static class UpdateTeamsNotificationHandler {
    public static void Handle(UpdateTeamsNotification response) {
        string msg;

        if (GameManager.PlayerIsHost()) {
            msg = response.status == 0
                ? "Teams have been set. The room has been locked."
                : "Teams have been cleared. The room has been unlocked.";
            GameManager.CurrentGame.gameSettings.hasManuallySetTeams = true;
        } else {
            msg = response.status == 0
                ? "The host has set the teams. The room has been locked."
                : "The host has cleared the teams. The room has been unlocked.";
            GameManager.CurrentGame.gameSettings.hasManuallySetTeams = false;
        }

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
    }
}

public class ClearTeamSettings : SendMessage {
    public string messageType = "ClearTeams";

    public int gameId;
    public RegisterTicket ticket;
}