using System;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using UnityEngine;

namespace UltraBINGO.Components;

public class BingoLevelData : MonoBehaviour
{
    public bool isClaimed = false;
    public string claimedTeam = "none";
    public string claimedPlayer = "none";
    
    public float timeRequirement = 0;
    public float styleRequirement = 0;
    
    public string levelName = "";
    
    public bool isAngryLevel = false;
    public string angryParentBundle = "";
    public string angryLevelId = "";
}