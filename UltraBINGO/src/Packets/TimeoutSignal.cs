using System.Threading.Tasks;
using UltraBINGO.API;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class TimeoutSignal : IncomingPacket {
    public required string Username;
    public required string SteamId;

    public override Task Handle() {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
            $"{GameManager.CurrentGame.CurrentPlayers[SteamId].Username} has lost connection to the game."
        );

        GameManager.CurrentGame.CurrentPlayers.Remove(SteamId);

        if (GetSceneName() == "Main Menu") GameManager.RefreshPlayerList();

        return Task.CompletedTask;
    }
}