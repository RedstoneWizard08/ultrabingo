using System.Collections.Generic;
using System.Linq;
using UltraBINGO.UI_Elements;

namespace UltraBINGO.NetworkMessages;

public class JoinRoomRequest : SendMessage {
    public string messageType = "JoinRoom";

    public string password;
    public string username;
    public string steamId;
    public string rank;
}

public class JoinRoomResponse : MessageResponse {
    public int status;
    public int roomId;
    public Game roomDetails;
}

public static class JoinRoomResponseHandler {
    public static Dictionary<int, string> messages = new() {
        { -6, "You have been kicked from this game." },
        { -5, "<color=orange>You are banned from playing Baphomet's Bingo.</color>" },
        { -4, "Game has already started." },
        { -3, "Game is not accepting new players." },
        { -2, "Game has already started." },
        { -1, "Game does not exist." }
    };

    public static void Handle(JoinRoomResponse response) {
        var msg = "Failed to join: ";

        if (response.status < 0) {
            msg += messages[response.status];
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
            NetworkManager.DisconnectWebSocket(1000, "Normal close");
        } else {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Joined game.");
            GameManager.SetupGameDetails(response.roomDetails, "", false);
        }

        BingoMainMenu.UnlockUI();
        BingoBrowser.UnlockUI();
    }
}