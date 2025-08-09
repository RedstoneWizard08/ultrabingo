namespace UltraBINGO.Types;

public record PublicGameData {
    public required string Password;
    public required int CurrentPlayers;
    public required string HostUsername;
    public required int MaxPlayers;
    public required int Difficulty;
}
