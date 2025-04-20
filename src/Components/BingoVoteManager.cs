using Newtonsoft.Json;
using TMPro;
using UltraBINGO.NetworkMessages;
using UltrakillBingoClient;
using UnityEngine;

namespace UltraBINGO.Components;

using static UltraBINGO.CommonFunctions;

public class BingoVoteManager : MonoSingleton<BingoVoteManager>
{
    public GameObject Panel;
    
    public float timeRemaining;
    public bool voteOngoing = false;
    public bool hasVoted = false;

    public string map = "";
    
    public int currentVotes = 0;
    public int voteThreshold = 0;
    
    public const string votePrompt = "(<color=orange>F1</color> to vote)";
    
    public static TextMeshProUGUI RerollText;
    public static TextMeshProUGUI RerollVotes;
    public static TextMeshProUGUI RerollTimer;
    
    public void Bind(GameObject source)
    {
        Panel = source;
    }
    public void CheckOngoingVote()
    {
        if (GameManager.voteData != null && GameManager.voteData.voteOngoing)
        {
            LoadOngoingVote();
        }
        else{Logging.Warn("No vote data");}
    }
    
    public void LoadOngoingVote()
    {
        Logging.Warn("Loading vote data");
        
        this.voteOngoing = true;
        timeRemaining = GameManager.voteData.timeLeft;
        hasVoted = GameManager.voteData.hasVoted;
        currentVotes = GameManager.voteData.currentVotes;
        voteThreshold = GameManager.voteData.minimumVotesRequired;
        map = GameManager.voteData.mapName;
        this.gameObject.SetActive(true);
        
    }
    
    public void Start()
    {
        RerollText = GetGameObjectChild(gameObject,"RerollText").GetComponent<TextMeshProUGUI>();
        RerollVotes = GetGameObjectChild(gameObject,"RerollVotes").GetComponent<TextMeshProUGUI>();
        RerollTimer = GetGameObjectChild(gameObject,"RerollTimer").GetComponent<TextMeshProUGUI>();
        
        //DontDestroyOnLoad(this.gameObject);
    }
    
    public void Update()
    {
        if(voteOngoing && timeRemaining > 0f)
        {
            timeRemaining = Mathf.MoveTowards(this.timeRemaining,0f,Time.unscaledDeltaTime);
            
            RerollText.text = "Reroll <color=orange>" + map + "</color>?" + (!hasVoted ? votePrompt : "");
            RerollVotes.text = "<color=orange>" + currentVotes + "</color>/" + voteThreshold + " votes";
            RerollTimer.text = "Ends in <color=orange>" + (int)timeRemaining + "</color>s";
        }
        else if (voteOngoing && timeRemaining == 0f)
        {
            stopVote();
        }
        
        if(Input.GetKeyDown(KeyCode.F1))
        {
            if(hasVoted)
            {
                Logging.Warn("Tried to vote, but alreadyVoted set to true!");
            }
            else
            {
                Logging.Warn("Creating vote request");
                RerollRequest rr = new RerollRequest();
                rr.gameId = GameManager.CurrentGame.gameId;
                rr.steamId = Steamworks.SteamClient.SteamId.ToString();
                rr.row = 0;
                rr.column = 0;
                rr.steamTicket = NetworkManager.CreateRegisterTicket();
        
                Logging.Warn("Creating vote request");
                NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(rr)); 
                Logging.Warn("Done, hasVoted to true");
                hasVoted = true;
            }
        }
    }
    
    public void stopVote()
    {
        timeRemaining = 0f;
        voteOngoing = false;
        hasVoted = false;
        gameObject.SetActive(false);
    }
    
    public void startVote(string voteStarter,float voteTime, string mapName, int votesRequired)
    {
        map = mapName;
        currentVotes = 1; 
        voteThreshold = votesRequired;
        if(voteStarter == Steamworks.SteamClient.SteamId.ToString()) {hasVoted = true; GameManager.alreadyStartedVote = true;}
        timeRemaining = voteTime;
        voteOngoing = true;
        gameObject.SetActive(true);
    }
    
    public void addVote()
    {
        currentVotes++;
    }
    
}

public class VoteData
{
    public float timeLeft;
    
    public bool voteOngoing;
    public bool hasVoted;
    
    public int minimumVotesRequired;
    public int currentVotes;
    
    public string mapName;
    
    public VoteData(bool ongoing)
    {
        this.voteOngoing = false;
    }
    public VoteData(bool ongoing, bool hasVoted, int minVotes, int curVotes, string mapName, float timeLeft)
    {
        this.timeLeft = timeLeft-1f; // Minus 1 second to account for 1s delay before switching maps
        this.voteOngoing = ongoing;
        this.hasVoted = hasVoted;
        this.minimumVotesRequired = minVotes;
        this.currentVotes = curVotes;
        this.mapName = mapName;
    }
}