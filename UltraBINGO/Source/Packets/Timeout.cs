using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class Timeout : IncomingPacket {
    [JsonProperty] public required string Player;
    [JsonProperty] public required string SteamId;

    public override void Handle() {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
            $"{GameManager.CurrentGame.CurrentPlayers[SteamId].Username} has lost connection to the game."
        );

        GameManager.CurrentGame.CurrentPlayers.Remove(SteamId);

        if (GetSceneName() == "Main Menu") GameManager.RefreshPlayerList();
    }
}