using Newtonsoft.Json;
using TMPro;
using UltraBINGO.Net;
using UltraBINGO.Packets;
using UltraBINGO.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltraBINGO.Components;

using static CommonFunctions;

public class BingoVoteManager : MonoSingleton<BingoVoteManager> {
    [FormerlySerializedAs("Panel")] public GameObject panel;

    public float timeRemaining;
    public bool voteOngoing;
    public bool hasVoted;

    public string map = "";

    public int currentVotes;
    public int voteThreshold;

    public const string VotePrompt = "(<color=orange>F1</color> to vote)";

    public static TextMeshProUGUI RerollText;
    public static TextMeshProUGUI RerollVotes;
    public static TextMeshProUGUI RerollTimer;

    public void Bind(GameObject source) {
        panel = source;
    }

    public void CheckOngoingVote() {
        if (GameManager.voteData != null && GameManager.voteData.VoteOngoing) LoadOngoingVote();
    }

    public void LoadOngoingVote() {
        voteOngoing = true;
        timeRemaining = GameManager.voteData.TimeLeft;
        hasVoted = GameManager.voteData.HasVoted;
        currentVotes = GameManager.voteData.CurrentVotes;
        voteThreshold = GameManager.voteData.MinimumVotesRequired;
        map = GameManager.voteData.MapName;
        gameObject.SetActive(true);
    }

    public void Start() {
        RerollText = GetGameObjectChild(gameObject, "RerollText").GetComponent<TextMeshProUGUI>();
        RerollVotes = GetGameObjectChild(gameObject, "RerollVotes").GetComponent<TextMeshProUGUI>();
        RerollTimer = GetGameObjectChild(gameObject, "RerollTimer").GetComponent<TextMeshProUGUI>();

        //DontDestroyOnLoad(this.gameObject);
    }

    public void Update() {
        if (voteOngoing && timeRemaining > 0f) {
            timeRemaining = Mathf.MoveTowards(timeRemaining, 0f, Time.unscaledDeltaTime);

            RerollText.text = $"Reroll <color=orange>{map}</color>?{(!hasVoted ? VotePrompt : "")}";
            RerollVotes.text = $"<color=orange>{currentVotes}</color>/{voteThreshold} votes";
            RerollTimer.text = $"Ends in <color=orange>{(int)timeRemaining}</color>s";
        } else if (voteOngoing && timeRemaining == 0f) {
            StopVote();
        }

        if (Input.GetKeyDown(KeyCode.F1)) {
            if (hasVoted) {
                Logging.Warn("Tried to vote, but alreadyVoted set to true!");
            } else {
                var rr = new RerollRequest {
                    GameId = GameManager.CurrentGame.GameId,
                    SteamId = Steamworks.SteamClient.SteamId.ToString(),
                    Row = 0,
                    Column = 0,
                    SteamTicket = NetworkManager.CreateRegisterTicket()
                };

                NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(rr));
                hasVoted = true;
            }
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