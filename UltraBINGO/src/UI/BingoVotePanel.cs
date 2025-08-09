using UltraBINGO.Components;
using UnityEngine;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI;

public static class BingoVotePanel {
    public static GameObject Root;
    public static GameObject RerollText;
    public static GameObject RerollVotes;
    public static GameObject RerollTimer;

    public static BingoVoteManager manager;

    public static void Init(ref GameObject votePanel) {
        RerollText = GetGameObjectChild(votePanel, "RerollText");
        RerollVotes = GetGameObjectChild(votePanel, "RerollVotes");
        RerollTimer = GetGameObjectChild(votePanel, "RerollTimer");
    }
}