using System.Collections.Generic;
using System.Linq;

namespace UltraBINGO.Types;

public class Game {
    public required int GameId;
    public required Dictionary<string, Player> CurrentPlayers; //<SteamID,Player>
    public required GameGrid Grid;
    public required string GameHost; //SteamID
    public required int GameState;
    public required GameSettings GameSettings;
    public required string WinningTeam;

    public List<Player> GetPlayers() {
        return CurrentPlayers.Values.ToList();
    }

    public bool IsGameFinished() {
        return GameState == 2;
    }
}