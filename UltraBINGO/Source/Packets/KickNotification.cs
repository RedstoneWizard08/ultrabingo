using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.Util;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class KickNotification : IncomingPacket {
    [JsonProperty] public required string PlayerToKick;
    [JsonProperty] public required string SteamId;
    
    public override void Handle() {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage($"{PlayerToKick} was kicked from the game.");
        GameManager.CurrentGame.CurrentPlayers.Remove(SteamId);
        
        if (CommonFunctions.GetSceneName() == "Main Menu") GameManager.RefreshPlayerList();
    }
}