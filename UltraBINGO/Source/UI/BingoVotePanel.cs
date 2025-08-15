using UltraBINGO.Components;
using UnityEngine;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.UI;

public static class BingoVotePanel {
    public static GameObject? Root;
    public static GameObject? RerollText;
    public static GameObject? RerollVotes;
    public static GameObject? RerollTimer;

    public static void Init(ref GameObject votePanel) {
        RerollText = FindObject(votePanel, "RerollText");
        RerollVotes = FindObject(votePanel, "RerollVotes");
        RerollTimer = FindObject(votePanel, "RerollTimer");
    }
}