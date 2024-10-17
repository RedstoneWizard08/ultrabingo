using System.Threading.Tasks;
using UltraBINGO.UI_Elements;
using UnityEngine;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.NetworkMessages;

public class EndGameSignal
{
    public string winningTeam;
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
        await Task.Delay(500);

        GameManager.CurrentGame.gameState = 2; // State 2 = game finished

        string message = "<color=orange>GAME OVER!</color> The " + response.winningTeam + " team has won the game!";
        GameManager.CurrentGame.winningTeam = response.winningTeam;
        if(getSceneName() != "Main Menu" && GameManager.isInBingoLevel)
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