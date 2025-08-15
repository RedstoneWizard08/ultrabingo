using TMPro;
using UltraBINGO.Util;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.UI;

public static class BingoCard {
    public static GameObject? Root;
    public static GameObject? Grid;
    public static GameObject? Teammates;
    private static GameObject? _buttonTemplate;
    private static GameObject? _leaveGame;
    private static GameObject? _teamIndicator;
    private static GameObject? _objectiveIndicator;
    private static GameObject? _cardElements;

    private const string Team = "PLACEHOLDER";

    public static void Cleanup() {
        if (Grid == null) return;

        foreach (Transform child in Grid.transform) {
            var toRemove = child.gameObject;
            Object.Destroy(toRemove);
        }
    }

    public static void UpdateTitles(int gameType) {
        var bingoDescription =
            $"Race to <color=orange>obtain the fastest time</color> for your team on each level.\nClaim {GameManager.CurrentGame.Grid.Size} levels horizontally, vertically or diagonally for your team to win!";

        const string dominationDescription =
            "Race to <color=orange>claim as many levels</color> for your team as possible.\nThe team with the most claims when time is up is the winner!";

        if (_teamIndicator != null)
            _teamIndicator.GetComponent<TMP_Text>().text =
                $"-- You are on the <color={GameManager.CurrentTeam.ToLower()}>{GameManager.CurrentTeam} team</color> --";

        if (_objectiveIndicator == null) return;

        _objectiveIndicator.GetComponent<TMP_Text>().text =
            gameType == 0 ? bingoDescription : dominationDescription;
        
        if (GameManager.CurrentGame.GameSettings.RequiresPRank)
            _objectiveIndicator.GetComponent<TMP_Text>().text +=
                "\n<color=#ffa200d9>P</color>-Ranks are <color=orange>required</color> to claim a level.";
    }

    public static GameObject Init() {
        if (Root == null) Root = new GameObject();
        
        Root.name = "UltraBingoCard";

        _cardElements = Object.Instantiate(AssetLoader.BingoCardElements, Root.transform);

        //Bingo grid
        Grid = FindObject(_cardElements, "BingoGrid");
        if (Grid != null) {
            Grid.name = "BingoGrid";
            Grid.transform.SetParent(Root.transform);
        }

        if (_buttonTemplate == null) _buttonTemplate = new GameObject();

        //Have to create the button with normal Text instead of TextMeshProUGUI as trying to instantiate an object with the latter causes crashes.
        _buttonTemplate = Object.Instantiate(AssetLoader.BingoCardButtonTemplate, Root.transform);
        
        if (_buttonTemplate != null) {
            _buttonTemplate.transform.SetParent(Root.transform);
            _buttonTemplate.SetActive(false);
        }

        _leaveGame = FindObject(_cardElements, "LeaveGame");
        _leaveGame?.GetComponent<Button>().onClick.AddListener(delegate { GameManager.LeaveGame().Wait(); });

        //Team indicator panel
        _teamIndicator = FindObject(_cardElements, "TeamIndicator");
        if (_teamIndicator != null)
            _teamIndicator.GetComponent<TextMeshProUGUI>().text = $"-- You are on the {Team} team -- ";

        //Time/style indicator panel
        _objectiveIndicator = FindObject(_cardElements, "ObjectiveIndicator");

        //Teammate panel
        Teammates = FindObject(_cardElements, "Teammates");

        Root.SetActive(false);
        return Root;
    }
}