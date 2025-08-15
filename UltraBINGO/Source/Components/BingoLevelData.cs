using UnityEngine;

// ReSharper disable InconsistentNaming

namespace UltraBINGO.Components;

public class BingoLevelData : MonoBehaviour {
    public bool IsClaimed;
    public string ClaimedTeam = "none";
    public string ClaimedPlayer = "none";
    public int Row;
    public int Column;
    public float TimeRequirement;
    public string LevelName = "";
    public bool IsAngryLevel;
    public string AngryParentBundle = "";
    public string AngryLevelId = "";
}