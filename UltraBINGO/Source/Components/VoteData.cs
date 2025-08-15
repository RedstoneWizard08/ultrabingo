namespace UltraBINGO.Components;

public class VoteData {
    public readonly float TimeLeft;
    public readonly bool VoteOngoing;
    public readonly bool HasVoted;
    public readonly int MinimumVotesRequired;
    public readonly int CurrentVotes;
    public readonly string? MapName;

    public VoteData(bool ongoing) {
        VoteOngoing = false;
    }

    public VoteData(bool ongoing, bool hasVoted, int minVotes, int curVotes, string? mapName, float timeLeft) {
        TimeLeft = timeLeft - 1f; // Minus 1 second to account for 1s delay before switching maps
        VoteOngoing = ongoing;
        HasVoted = hasVoted;
        MinimumVotesRequired = minVotes;
        CurrentVotes = curVotes;
        MapName = mapName;
    }
}