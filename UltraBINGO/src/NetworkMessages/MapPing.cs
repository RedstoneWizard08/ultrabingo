using UltraBINGO.Components;
using UltraBINGO.UI_Elements;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.NetworkMessages;

public class MapPing : SendMessage {
    public string messageType = "MapPing";

    public int gameId;

    public string team;
    public int row;
    public int column;

    public RegisterTicket ticket;
}

public class MapPingNotification : MessageResponse {
    public int row;
    public int column;
}

public static class MapPingNotificationHandler {
    public static void Handle(MapPingNotification response) {
        var location = response.row + "-" + response.column;

        var levelName = GetGameObjectChild(BingoCardPauseMenu.Grid, location).GetComponent<BingoLevelData>().levelName;

        if (BingoCardPauseMenu.pingedMap != null) Object.Destroy(BingoCardPauseMenu.pingedMap);

        var pingOutline = GetGameObjectChild(BingoCardPauseMenu.Grid, location).AddComponent<Outline>();
        pingOutline.effectColor = new Color(1f, 0.5f, 0f);
        pingOutline.effectDistance = new Vector2(2f, -2f);

        BingoCardPauseMenu.pingedMap = pingOutline;

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Your team has pinged <color=orange>" + levelName +
                                                                  "</color>.");
    }
}