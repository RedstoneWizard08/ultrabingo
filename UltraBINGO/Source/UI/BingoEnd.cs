using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.UI;

public static class BingoEnd {
    private static GameObject? _root;
    private static GameObject? _winnerIndicator;
    private static GameObject? _winningPlayers;
    private static GameObject? _stats;
    private static GameObject? _leaveGame;

    public static string? winningTeam;
    public static string? winningPlayers;
    public static string? timeElapsed;
    public static int numOfClaims;
    public static string? firstMap;
    public static string? lastMap;
    public static float bestStatValue;
    public static string? bestStatName;

    public static int endCode;
    public static string? tiedTeams;

    public static async Task ShowEndScreen() {
        await Task.Delay(50); //Give the game a moment to fully load back into the menu before displaying

        FindObjectWithInactiveRoot("Canvas", "Main Menu (1)")?.SetActive(false);
        
        BingoEncapsulator.Root?.SetActive(true);
        BingoEncapsulator.BingoMenu?.SetActive(false);
        BingoEncapsulator.BingoLobbyScreen?.SetActive(false);
        BingoEncapsulator.BingoEndScreen?.SetActive(true);

        var message = endCode switch {
            0 => $"The <color={winningTeam?.ToLower()}>{winningTeam} </color>team has won the game!",
            1 => "No team won the game.",
            2 => $"The {tiedTeams} teams have tied!",
            _ => ""
        };

        if (_winnerIndicator != null) _winnerIndicator.GetComponent<TextMeshProUGUI>().text = message;

        FindObject(_winningPlayers, "Text (TMP) (1)")?.GetComponent<TextMeshProUGUI>().SetText(winningPlayers);

        FindObject(_stats, "TimeElapsed", "Value")?.GetComponent<TextMeshProUGUI>()
            .SetText($"<color=orange>{timeElapsed}</color>");

        FindObject(_stats, "TotalClaims", "Value")?.GetComponent<TextMeshProUGUI>()
            .SetText($"<color=orange>{numOfClaims}</color>");

        FindObject(_stats, "FirstMap", "Value")?.GetComponent<TextMeshProUGUI>()
            .SetText($"<color=orange>{firstMap}</color>");

        FindObject(_stats, "LastMap", "Value")?.GetComponent<TextMeshProUGUI>()
            .SetText($"<color=orange>{lastMap}</color>");

        FindObject(_stats, "HighestStat", "Value")?.GetComponent<TextMeshProUGUI>().SetText(
            $"<color=orange>{bestStatValue} </color>(<color=orange>{bestStatName}</color>)"
        );

        _root?.SetActive(true);
    }

    public static void Init(ref GameObject bingoEndScreen) {
        if (_root == null) _root = new GameObject();
        _root.name = "BingoEndScreen";

        _winnerIndicator = FindObject(bingoEndScreen, "WinningTeam");
        _winningPlayers = FindObject(bingoEndScreen, "WinningPlayers");
        _stats = FindObject(bingoEndScreen, "Stats");
        _leaveGame = FindObject(bingoEndScreen, "LeaveGame");

        _leaveGame?.GetComponent<Button>().onClick.AddListener(
            delegate {
                GameManager.LeaveGame().Wait();
                BingoMapSelection.ClearList(true);
            }
        );

        _leaveGame?.transform.SetParent(bingoEndScreen.transform);
        bingoEndScreen.SetActive(false);
    }
}