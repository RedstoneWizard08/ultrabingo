using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UltraBINGO.UI_Elements;
using UnityEngine;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.NetworkMessages;

public class EndGameSignal
{
    public string winningTeam;
    public List<String> winningPlayers;
    public string timeElapsed;
    public int claims;
    public string firstMapClaimed;
    public string lastMapClaimed;
    
    public float bestStatValue;
    public string bestStatMap;
}

public static class EndGameSignalHandler
{
    public static void PlayEndSound()
    {
        GameObject go = new GameObject();
        Transform transform = MonoSingleton<NewMovement>.Instance.transform;
        GameObject sound = GameObject.Instantiate(go, transform.position, Quaternion.identity, transform);
        sound.AddComponent<AudioSource>();
        sound.GetComponent<AudioSource>().playOnAwake = false;
        sound.GetComponent<AudioSource>().clip = AssetLoader.GameOverSound;
        sound.GetComponent<AudioSource>().Play();
    }
    
    public static async void handle(EndGameSignal response)
    {
        BingoEnd.winningTeam = response.winningTeam;
        BingoEnd.winningPlayers = string.Join(",",response.winningPlayers);
        BingoEnd.timeElapsed = response.timeElapsed;
        BingoEnd.numOfClaims = response.claims;
        BingoEnd.firstMap = response.firstMapClaimed;
        BingoEnd.lastMap = response.lastMapClaimed;
        
        BingoEnd.bestStatValue = response.bestStatValue;
        BingoEnd.bestStatName = response.bestStatMap;
        
        await Task.Delay(250);

        GameManager.CurrentGame.gameState = 2; // State 2 = game finished

        string message = "<color=orange>GAME OVER!</color> The <color="+ response.winningTeam.ToLower() +">"+ response.winningTeam + " </color>team has <color=orange>won the game</color>!";
        GameManager.CurrentGame.winningTeam = response.winningTeam;
        if(getSceneName() != "Main Menu" && GameManager.IsInBingoLevel)
        {
            PlayEndSound();
            
            MonoSingleton<MusicManager>.Instance.ForceStopMusic();
            MonoSingleton<AssistController>.Instance.majorEnabled = true;
            MonoSingleton<AssistController>.Instance.gameSpeed = 0.35f; 
            message += "\n Exiting mission in 5 seconds...";
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(message);
            await Task.Delay(5000);
            SceneHelper.LoadScene("Main Menu");
        }
        else
        {
            message += "\n Displaying results in 5 seconds...";
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(message);
            await Task.Delay(5000);
            BingoEnd.ShowEndScreen();
        }
    }
}