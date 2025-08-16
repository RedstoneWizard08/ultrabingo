using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.UI;
using UltraBINGO.Util;
using UnityEngine;
using static UltraBINGO.Util.CommonFunctions;
using Object = UnityEngine.Object;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class GameEnd : IncomingPacket {
    [JsonProperty] public required string WinningTeam;
    [JsonProperty] public required List<string> WinningPlayers;
    [JsonProperty] public required string TimeElapsed;
    [JsonProperty] public required int Claims;
    [JsonProperty] public string? FirstMapClaimed;
    [JsonProperty] public string? LastMapClaimed;
    [JsonProperty] public float? BestStatValue;
    [JsonProperty] public string? BestStatMap;
    [JsonProperty] public required int EndStatus;
    [JsonProperty] public required List<string> TiedTeams;

    private static void PlayEndSound() {
        var go = new GameObject();
        var transform = MonoSingleton<NewMovement>.Instance.transform;
        var sound = Object.Instantiate(go, transform.position, Quaternion.identity, transform);

        sound.AddComponent<AudioSource>();
        sound.GetComponent<AudioSource>().playOnAwake = false;
        sound.GetComponent<AudioSource>().clip = AssetLoader.GameOverSound;
        sound.GetComponent<AudioSource>().Play();
    }

    public override void Handle() {
        BingoEnd.winningTeam = EndStatus switch {
            0 => WinningTeam,
            2 => string.Join("&", WinningPlayers),
            _ => ""
        };

        BingoEnd.winningPlayers = EndStatus == 0 ? string.Join(",", WinningPlayers) : "";
        BingoEnd.timeElapsed = TimeElapsed;
        BingoEnd.numOfClaims = Claims;
        BingoEnd.firstMap = FirstMapClaimed;
        BingoEnd.lastMap = LastMapClaimed;
        BingoEnd.bestStatValue = BestStatValue;
        BingoEnd.bestStatName = BestStatMap;
        BingoEnd.endCode = EndStatus;

        if (EndStatus == 2) BingoEnd.tiedTeams = string.Join(" & ", TiedTeams);

        Thread.Sleep(250);

        GameManager.CurrentGame.GameState = 2; // State 2 = game finished

        var message = "<color=orange>GAME OVER!</color>";

        message += EndStatus switch {
            0 => $"The {string.Join("&", TiedTeams)} teams have <color=orange>tied for the win!</color>", // Normal
            1 => "No winning team has been declared.", // No winner
            2 => $"The {string.Join("&", TiedTeams)} teams have <color=orange>tied for the win!</color>", // Tie
            var other => throw new ArgumentOutOfRangeException($"Unknown game end status: {other}")
        };

        GameManager.CurrentGame.WinningTeam = WinningTeam;

        if (GetSceneName() != "Main Menu" && GameManager.IsInBingoLevel) {
            PlayEndSound();

            MonoSingleton<MusicManager>.Instance.ForceStopMusic();
            MonoSingleton<AssistController>.Instance.majorEnabled = true;
            MonoSingleton<AssistController>.Instance.gameSpeed = 0.35f;

            message += "\n Exiting mission in 5 seconds...";

            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(message);

            Thread.Sleep(5000);

            SceneHelper.LoadScene("Main Menu");
        } else {
            message += "\n Displaying results in 5 seconds...";

            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(message);

            Thread.Sleep(5000);

            BingoEnd.ShowEndScreen();
        }
    }
}