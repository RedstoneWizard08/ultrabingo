using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.Util;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class LevelClaimed : IncomingPacket {
    [JsonProperty] public required int ClaimType; // 0: Claimed, 1: Improved, 2: Reclaimed
    [JsonProperty] public required string Username;
    [JsonProperty] public required string LevelName;
    [JsonProperty] public required string Team;
    [JsonProperty] public required int Row;
    [JsonProperty] public required int Column;
    [JsonProperty] public required float NewTimeRequirement;
    [JsonProperty] public required bool IsMapVoted;

    public override void Handle() {
        try {
            var actionType = ClaimType switch {
                0 => "claimed ",
                1 => "improved ",
                2 => "reclaimed ",
                _ => ""
            };

            var secs = NewTimeRequirement;
            float mins = 0;

            while (secs >= 60f) {
                secs -= 60f;
                mins += 1f;
            }

            var formattedTime = $"{mins}:{secs:00.000}";

            var broadcastString =
                $"<color={Team.ToLower()}>{Username}</color> has <color=orange>{actionType}{LevelName}</color> for the <color={Team.ToLower()}>{Team} </color>team (<color=orange>{formattedTime}</color>).";

            if (IsMapVoted) broadcastString += "\n Cancelling reroll vote.";

            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(broadcastString);
            GameManager.UpdateCards(Row, Column, Team, Username, NewTimeRequirement);
        } catch (Exception e) {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                "A level was claimed by someone but was unable to update the grid.\nCheck BepInEx console and report it to Clearwater!");

            Logging.Error(e.Message);

            throw;
        }
    }
}