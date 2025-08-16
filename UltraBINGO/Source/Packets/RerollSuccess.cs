using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.Components;
using UltraBINGO.Types;
using UltraBINGO.Util;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class RerollSuccess : IncomingPacket {
    [JsonProperty] public required string OldMapId;
    [JsonProperty] public required string OldMapName;
    [JsonProperty] public required GameLevel MapData;
    [JsonProperty] public required int LocationX;
    [JsonProperty] public required int LocationY;

    public override void Handle() {
        MonoSingleton<BingoVoteManager>.Instance.StopVote();

        var msg =
            $"VOTE SUCCESSFUL - <color=orange>{OldMapName} </color>has rerolled to <color=orange>{MapData.LevelName}</color>.";

        if (OldMapId == CommonFunctions.GetSceneName()) msg += "\nChanging levels in 5 seconds...";

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);

        GameManager.SwapRerolledMap(OldMapId, MapData, LocationX, LocationY);
    }
}