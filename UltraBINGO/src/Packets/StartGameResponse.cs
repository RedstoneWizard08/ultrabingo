using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.Types;
using UltraBINGO.UI;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class StartGameResponse : IncomingPacket {
    [JsonProperty] public required Game Game;
    [JsonProperty] public required string TeamColor;
    [JsonProperty] public required List<string> Teammates;
    [JsonProperty] public required GameGrid Grid;

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