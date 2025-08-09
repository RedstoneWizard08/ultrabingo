using System.Threading.Tasks;
using UltraBINGO.API;
using UltraBINGO.Components;
using UltraBINGO.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class MapPingNotification : IncomingPacket {
    public required int Row;
    public required int Column;

    public override Task Handle() {
        var location = $"{Row}-{Column}";

        var levelName = CommonFunctions.GetGameObjectChild(BingoCardPauseMenu.Grid, location)?.GetComponent<BingoLevelData>().LevelName;

        if (BingoCardPauseMenu.pingedMap != null) Object.Destroy(BingoCardPauseMenu.pingedMap);

        var pingOutline = CommonFunctions.GetGameObjectChild(BingoCardPauseMenu.Grid, location)?.AddComponent<Outline>();

        if (pingOutline != null) {
            pingOutline.effectColor = new Color(1f, 0.5f, 0f);
            pingOutline.effectDistance = new Vector2(2f, -2f);

            BingoCardPauseMenu.pingedMap = pingOutline;
        }

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
            $"Your team has pinged <color=orange>{levelName}</color>.");

        return Task.CompletedTask;
    }
}