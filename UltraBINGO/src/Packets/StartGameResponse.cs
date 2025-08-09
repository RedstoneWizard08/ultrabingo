using System.Collections.Generic;
using System.Threading.Tasks;
using UltraBINGO.API;
using UltraBINGO.Types;
using UltraBINGO.UI;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class StartGameResponse : IncomingPacket {
    public required Game Game;
    public required string TeamColor;
    public required List<string> Teammates;
    public required GameGrid Grid;

    public override Task Handle() {
        GameManager.CurrentTeam = TeamColor;
        GameManager.Teammates = Teammates;
        GameManager.CurrentGame.Grid = Grid;

        GameManager.dominationTimer = Game.GameSettings.Gamemode switch {
            1 => Game.GameSettings.TimeLimit * 60, // Domination
            _ => GameManager.dominationTimer
        };

        BingoMenuController.StartGame(Game.GameSettings.Gamemode);

        return Task.CompletedTask;
    }
}