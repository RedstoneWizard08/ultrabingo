using System.Threading.Tasks;
using UltraBINGO.API;
using UltraBINGO.UI;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class Kicked : IncomingPacket {
    public override Task Handle() {
        GameManager.ClearGameVariables();
        
        // If dc'ing from lobby/card/end screen, return to the bingo menu.
        BingoEncapsulator.BingoCardScreen?.SetActive(false);
        BingoEncapsulator.BingoLobbyScreen?.SetActive(false);
        BingoEncapsulator.BingoEndScreen?.SetActive(false);
        BingoEncapsulator.BingoMenu?.SetActive(true);
        
        return Task.CompletedTask;
    }
}