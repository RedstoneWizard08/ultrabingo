using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.Components;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class RerollVoteNotification : IncomingPacket {
    [JsonProperty] public required string MapName;
    [JsonProperty] public required string VoteStarter;
    [JsonProperty] public required string VoteStarterSteamId;
    [JsonProperty] public required string NumVotes;
    [JsonProperty] public required int VotesRequired;
    [JsonProperty] public required int NotifType;
    [JsonProperty] public required int Timer;

    public override Task Handle() {
        var type = NotifType == 0 ? " started a vote to reroll " : " voted to reroll ";

        var msg =
            $"{VoteStarter}{type}<color=orange>{MapName}</color>. ({NumVotes}/{VotesRequired} votes)";

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);

        var manager = MonoSingleton<BingoVoteManager>.Instance;

        if (NotifType == 0) {
            manager.Start();
            manager.StartVote(
                VoteStarterSteamId,
                Timer + 1,
                MapName,
                VotesRequired
            );
        } else {
            manager.AddVote();
        }

        GameManager.VoteData = new VoteData(
            true,
            manager.hasVoted,
            manager.voteThreshold,
            manager.currentVotes,
            manager.map,
            manager.timeRemaining
        );

        return Task.CompletedTask;
    }
}