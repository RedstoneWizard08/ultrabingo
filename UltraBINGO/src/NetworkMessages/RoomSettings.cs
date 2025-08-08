﻿using UltraBINGO.UI_Elements;

namespace UltraBINGO.NetworkMessages;

public class UpdateRoomSettingsRequest : SendMessage {
    public string messageType = "UpdateRoomSettings";

    public int roomId;

    public int maxPlayers;
    public int maxTeams;
    public int timeLimit;
    public int teamComposition;
    public int gamemode;
    public bool PRankRequired;
    public int difficulty;
    public int gridSize;
    public bool disableCampaignAltExits;
    public int gameVisibility;

    public RegisterTicket ticket;
}

public class UpdateRoomSettingsNotification : MessageResponse {
    public int maxPlayers;
    public int maxTeams;
    public int timeLimit;
    public int teamComposition;
    public int gamemode;
    public bool PRankRequired;
    public int difficulty;
    public int gridSize;
    public bool disableCampaignAltExits;
    public int gameVisibility;

    public bool wereTeamsReset;
}

public static class UpdateRoomSettingsHandler {
    public static void Handle(UpdateRoomSettingsNotification response) {
        BingoLobby.UpdateFromNotification(response);
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("The host has updated the room settings.");
    }
}