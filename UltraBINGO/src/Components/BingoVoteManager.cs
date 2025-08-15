using TMPro;
using UltraBINGO.Net;
using UltraBINGO.Packets;
using UltraBINGO.Util;
using UnityEngine;

namespace UltraBINGO.Components;

using static CommonFunctions;

public class BingoVoteManager : MonoSingleton<BingoVoteManager> {
    public float timeRemaining;
    public bool voteOngoing;
    public bool hasVoted;
    public string map = "";
    public int currentVotes;
    public int voteThreshold;

    private const string VotePrompt = "(<color=orange>F1</color> to vote)";

    private static TextMeshProUGUI? _rerollText;
    private static TextMeshProUGUI? _rerollVotes;
    private static TextMeshProUGUI? _rerollTimer;

    public void CheckOngoingVote() {
        if (GameManager.VoteData != null && GameManager.VoteData.VoteOngoing) LoadOngoingVote();
    }

    public void LoadOngoingVote() {
        if (GameManager.VoteData == null) return;

        voteOngoing = true;
        timeRemaining = GameManager.VoteData.TimeLeft;
        hasVoted = GameManager.VoteData.HasVoted;
        currentVotes = GameManager.VoteData.CurrentVotes;
        voteThreshold = GameManager.VoteData.MinimumVotesRequired;
        map = GameManager.VoteData.MapName;
        gameObject.SetActive(true);
    }

    public void Start() {
        _rerollText = GetGameObjectChild(gameObject, "RerollText")?.GetComponent<TextMeshProUGUI>();
        _rerollVotes = GetGameObjectChild(gameObject, "RerollVotes")?.GetComponent<TextMeshProUGUI>();
        _rerollTimer = GetGameObjectChild(gameObject, "RerollTimer")?.GetComponent<TextMeshProUGUI>();

        //DontDestroyOnLoad(this.gameObject);
    }

    public void Update() {
        switch (voteOngoing) {
            case true when timeRemaining > 0f:
                timeRemaining = Mathf.MoveTowards(timeRemaining, 0f, Time.unscaledDeltaTime);

                if (_rerollText != null)
                    _rerollText.text = $"Reroll <color=orange>{map}</color>?{(!hasVoted ? VotePrompt : "")}";

                if (_rerollVotes != null)
                    _rerollVotes.text = $"<color=orange>{currentVotes}</color>/{voteThreshold} votes";

                if (_rerollTimer != null) _rerollTimer.text = $"Ends in <color=orange>{(int)timeRemaining}</color>s";

                break;

            case true when timeRemaining == 0f:
                StopVote();
                break;
        }

        if (!Input.GetKeyDown(KeyCode.F1)) return;

        if (hasVoted) {
            Logging.Warn("Tried to vote, but alreadyVoted set to true!");
        } else {
            NetworkManager.SendEncodedMessage(
                new RerollRequest {
                    GameId = GameManager.CurrentGame.GameId,
                    SteamId = Steamworks.SteamClient.SteamId.ToString(),
                    Row = 0,
                    Column = 0,
                    SteamTicket = NetworkManager.CreateRegisterTicket()
                }
            ).Wait();

            hasVoted = true;
        }
    }

    public void StopVote() {
        timeRemaining = 0f;
        voteOngoing = false;
        hasVoted = false;
        gameObject.SetActive(false);
    }

    public void StartVote(string voteStarter, float voteTime, string mapName, int votesRequired) {
        map = mapName;
        currentVotes = 1;
        voteThreshold = votesRequired;
        if (voteStarter == Steamworks.SteamClient.SteamId.ToString()) {
            hasVoted = true;
            GameManager.alreadyStartedVote = true;
        }

        timeRemaining = voteTime;
        voteOngoing = true;
        gameObject.SetActive(true);
    }

    public void AddVote() {
        currentVotes++;
    }
}