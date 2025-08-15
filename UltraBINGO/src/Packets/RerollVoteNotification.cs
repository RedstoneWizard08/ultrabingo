using System.Threading.Tasks;
using UltraBINGO.API;
using UltraBINGO.Components;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class RerollVoteNotification : IncomingPacket {
    public required string MapName;
    public required string VoteStarter;
    public required string VoteStarterSteamId;
    public required string NumVotes;
    public required int VotesRequired;
    public required int NotifType;
    public required int Timer;

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