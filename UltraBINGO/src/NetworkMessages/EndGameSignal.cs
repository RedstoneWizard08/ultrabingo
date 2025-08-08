using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UltraBINGO.UI_Elements;
using UnityEngine;
using static UltraBINGO.CommonFunctions;
using Object = UnityEngine.Object;

namespace UltraBINGO.NetworkMessages;

public class EndGameSignal {
    public string winningTeam;
    public List<string> winningPlayers;
    public string timeElapsed;
    public int claims;
    public string firstMapClaimed;
    public string lastMapClaimed;

    public float bestStatValue;
    public string bestStatMap;

    public int endStatus;
    public List<string> tiedTeams;
}

public static class EndGameSignalHandler {
    public static void PlayEndSound() {
        var go = new GameObject();
        var transform = MonoSingleton<NewMovement>.Instance.transform;
        var sound = Object.Instantiate(go, transform.position, Quaternion.identity, transform);
        sound.AddComponent<AudioSource>();
        sound.GetComponent<AudioSource>().playOnAwake = false;
        sound.GetComponent<AudioSource>().clip = AssetLoader.GameOverSound;
        sound.GetComponent<AudioSource>().Play();
    }

    public static async void Handle(EndGameSignal response) {
        BingoEnd.winningTeam = response.endStatus == 0 ? response.winningTeam :
            response.endStatus == 2 ? string.Join("&", response.winningPlayers) : "";
        BingoEnd.winningPlayers = response.endStatus == 0 ? string.Join(",", response.winningPlayers) : "";
        BingoEnd.timeElapsed = response.timeElapsed;
        BingoEnd.numOfClaims = response.claims;
        BingoEnd.firstMap = response.firstMapClaimed;
        BingoEnd.lastMap = response.lastMapClaimed;

        BingoEnd.bestStatValue = response.bestStatValue;
        BingoEnd.bestStatName = response.bestStatMap;
        BingoEnd.endCode = response.endStatus;

        if (response.endStatus == 2) BingoEnd.tiedTeams = string.Join(" & ", response.tiedTeams);

        await Task.Delay(250);

        GameManager.CurrentGame.gameState = 2; // State 2 = game finished

        var message = "<color=orange>GAME OVER!</color>";
        switch (response.endStatus) {
            case 0: //Normal end
            {
                message += "The <color=" + response.winningTeam.ToLower() + ">" + response.winningTeam +
                           " </color>team has <color=orange>won the game</color>!";
                break;
            }
            case 1: //Game end with no winner
            {
                message += "No winning team has been declared.";
                break;
            }
            case 2: //Game end with tie
            {
                message += "The " + string.Join("&", response.tiedTeams) +
                           " teams have <color=orange>tied for the win!</color>";
                break;
            }
            default: {
                break;
            }
        }

        GameManager.CurrentGame.winningTeam = response.winningTeam;
        if (GetSceneName() != "Main Menu" && GameManager.IsInBingoLevel) {
            PlayEndSound();

            MonoSingleton<MusicManager>.Instance.ForceStopMusic();
            MonoSingleton<AssistController>.Instance.majorEnabled = true;
            MonoSingleton<AssistController>.Instance.gameSpeed = 0.35f;
            message += "\n Exiting mission in 5 seconds...";
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(message);
            await Task.Delay(5000);
            SceneHelper.LoadScene("Main Menu");
        } else {
            message += "\n Displaying results in 5 seconds...";
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(message);
            await Task.Delay(5000);
            BingoEnd.ShowEndScreen();
        }
    }
}