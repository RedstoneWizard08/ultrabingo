using System.Threading.Tasks;
using UltraBINGO.API;
using UltraBINGO.Types;
using UltraBINGO.UI;
using UltraBINGO.Util;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class CreateRoomResponse : IncomingPacket {
    public required string Status;
    public required int RoomId;
    public required Game RoomDetails;
    public required string RoomPassword;
    
    public override async Task Handle() {
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
                await GameManager.SetupGameDetails(RoomDetails, RoomPassword);
                
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage($"Joined game: {RoomId}");
            }
        }

        BingoMainMenu.UnlockUI();
        BingoBrowser.UnlockUI();
    }
}