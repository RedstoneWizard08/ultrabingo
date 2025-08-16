using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class UpdateTeams : IncomingPacket {
    [JsonProperty] public required int Status;

    public override void Handle() {
        string msg;

        if (GameManager.PlayerIsHost()) {
            msg = Status == 0
                ? "Teams have been set. The room has been locked."
                : "Teams have been cleared. The room has been unlocked.";

            GameManager.CurrentGame.GameSettings.HasManuallySetTeams = true;
        } else {
            msg = Status == 0
                ? "The host has set the teams. The room has been locked."
                : "The host has cleared the teams. The room has been unlocked.";

            GameManager.CurrentGame.GameSettings.HasManuallySetTeams = false;
        }

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
    }
}