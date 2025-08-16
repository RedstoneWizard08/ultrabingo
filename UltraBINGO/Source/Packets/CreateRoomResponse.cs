using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.Types;
using UltraBINGO.UI;
using UltraBINGO.Util;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class CreateRoomResponse : IncomingPacket {
    [JsonProperty] public required string Status;
    [JsonProperty] public required int RoomId;
    [JsonProperty] public Game? RoomDetails;
    [JsonProperty] public required string RoomPassword;
    
    public override void Handle() {
        if (Status == "ban") {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                "<color=orange>You have been banned from playing Baphomet's Bingo.</color>");
        } else {
            if (RoomId == 0) {
                //Was unable to create room
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to create room.");
            } else {
                Logging.Message($"Got details for room {RoomId}");

                //Once room details have been obtained: set up the lobby screen
                if (RoomDetails != null) GameManager.SetupGameDetails(RoomDetails, RoomPassword);

                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage($"Joined game: {RoomId}");
            }
        }

        BingoMainMenu.UnlockUI();
        BingoBrowser.UnlockUI();
    }
}