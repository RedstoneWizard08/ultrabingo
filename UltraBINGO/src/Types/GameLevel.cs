namespace UltraBINGO.Types;

public class GameLevel {
    public required string LevelName;
    public required string LevelId;
    public required string ClaimedBy;
    public required string PersonToBeat;
    public required float TimeToBeat;
    public required int Row;
    public required int Column;
    public required bool IsAngryLevel;
    public required string AngryParentBundle;
    public required string AngryLevelId;
}