using System.Threading.Tasks;
using UltraBINGO.API;
using UltraBINGO.Types;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class ReconnectResponse : IncomingPacket {
    public required string Status;
    public required Game GameData;

    public override async Task Handle() {
        switch (Status) {
            case "OK": {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Reconnected successfully.");
                break;
            }
            case "end": {
                await new DisconnectSignal {
                    DisconnectMessage = "GAMEENDED"
                }.Handle();
                
                //MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("The game has already ended.");
                
                break;
            }
            default: {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                    "Failed to reconnect. Leaving game in 5 seconds...");
                GameManager.ClearGameVariables();
                await Task.Delay(5000);

                SceneHelper.LoadScene("Main Menu");
                break;
            }
        }
    }
}