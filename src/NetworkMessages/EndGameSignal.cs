using System.Threading.Tasks;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

using static UltraBINGO.CommonFunctions;

public class EndGameSignal
{
    public string winningTeam;
}

public static class EndGameSignalHandler
{
    public static async void handle(EndGameSignal response)
    {
        string message = "<color=orange>GAME OVER!</color> The " + response.winningTeam + " team has won the game!";
        if(getSceneName() != "Main Menu" && GameManager.isInBingoLevel)
        {
            message += "\n Exiting mission in 5 seconds...";
        }
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(message);
        await Task.Delay(5000);
        SceneHelper.LoadScene("Main Menu");
    }
}