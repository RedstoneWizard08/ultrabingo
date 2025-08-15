using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.Net;
using UltraBINGO.Types;
using UltraBINGO.UI;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class JoinRoomResponse : IncomingPacket {
    [JsonProperty] public required int Status;
    [JsonProperty] public required int RoomId;
    [JsonProperty] public required Game RoomDetails;

    private static readonly Dictionary<int, string> Messages = new() {
        { -6, "You have been kicked from this game." },
        { -5, "<color=orange>You are banned from playing Baphomet's Bingo.</color>" },
        { -4, "Game has already started." },
        { -3, "Game is not accepting new players." },
        { -2, "Game has already started." },
        { -1, "Game does not exist." }
    };

    public override async Task Handle() {
        var msg = "Failed to join: ";

        if (Status < 0) {
            msg += Messages[Status];
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
            Main.NetworkManager.Socket.Disconnect(1000, "Normal close");
        } else {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Joined game.");
            await GameManager.SetupGameDetails(RoomDetails, "", false);
        }

        BingoMainMenu.UnlockUI();
        BingoBrowser.UnlockUI();
    }
}