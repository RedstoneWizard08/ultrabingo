using System.Threading.Tasks;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class KickNotification : IncomingPacket {
    public required string PlayerToKick;
    public required string SteamId;
    
    public override Task Handle() {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage($"{PlayerToKick} was kicked from the game.");
        GameManager.CurrentGame.CurrentPlayers.Remove(SteamId);
        
        if (CommonFunctions.GetSceneName() == "Main Menu") GameManager.RefreshPlayerList();
        
        return Task.CompletedTask;
    }
}