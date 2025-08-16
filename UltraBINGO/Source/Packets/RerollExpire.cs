using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class RerollExpire : IncomingPacket {
    [JsonProperty] public required string MapName;

    public override void Handle() {
        var msg = $"Vote to reroll <color=orange>{MapName}</color> has expired.";

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
    }
}