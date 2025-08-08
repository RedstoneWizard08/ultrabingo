using System;
using UltraBINGO.UI_Elements;
using UnityEngine;

namespace UltraBINGO.Components;

public class BingoLevelData : MonoBehaviour {
    public bool isClaimed;
    public string claimedTeam = "none";
    public string claimedPlayer = "none";

    public int row;
    public int column;

    public float timeRequirement;

    public string levelName = "";

    public bool isAngryLevel;
    public string angryParentBundle = "";
    public string angryLevelId = "";
}