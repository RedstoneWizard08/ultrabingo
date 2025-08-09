namespace UltraBINGO.Components;

public class VoteData {
    public float TimeLeft;

    public bool VoteOngoing;
    public bool HasVoted;

    public int MinimumVotesRequired;
    public int CurrentVotes;

    public string MapName;

    public VoteData(bool ongoing) {
        VoteOngoing = false;
    }

    public VoteData(bool ongoing, bool hasVoted, int minVotes, int curVotes, string mapName, float timeLeft) {
        this.TimeLeft = timeLeft - 1f; // Minus 1 second to account for 1s delay before switching maps
        VoteOngoing = ongoing;
        this.HasVoted = hasVoted;
        MinimumVotesRequired = minVotes;
        CurrentVotes = curVotes;
        this.MapName = mapName;
    }
}