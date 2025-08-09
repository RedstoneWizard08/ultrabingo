using System.Threading.Tasks;
using UltraBINGO.API;
using UltraBINGO.Components;
using UltraBINGO.Types;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class RerollSuccessNotification : IncomingPacket {
    public required string OldMapId;
    public required string OldMapName;
    public required GameLevel MapData;
    public required int LocationX;
    public required int LocationY;

    public override async Task Handle() {
        MonoSingleton<BingoVoteManager>.Instance.StopVote();

        var msg =
            $"VOTE SUCCESSFUL - <color=orange>{OldMapName} </color>has rerolled to <color=orange>{MapData.LevelName}</color>.";

        if (OldMapId == CommonFunctions.GetSceneName()) msg += "\nChanging levels in 5 seconds...";

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);

        await GameManager.SwapRerolledMap(OldMapId, MapData, LocationX, LocationY);
    }
}