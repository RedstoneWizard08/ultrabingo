using System.Collections.Generic;
using TMPro;
using UltraBINGO.Net;
using UltraBINGO.Types;
using UltraBINGO.Util;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.UI;

public static class BingoBrowser {
    private static GameObject? _fetchText;
    private static GameObject? _gameTemplate;
    private static GameObject? _gameListWrapper;
    private static GameObject? _gameList;
    private static GameObject? _back;
    private static readonly List<GameObject> GameArray = [];

    public static bool fetchDone;

    private static void LockUI() {
        //Lock all the join buttons of any games in the browser.

        foreach (var game in GameArray) {
            FindObject(game, "JoinWrapper", "JoinButton")?.GetComponent<Button>().SetInteractable(false);
        }
    }

    public static void UnlockUI() {
        //Unlock all the join buttons of any games in the browser.
        foreach (var game in GameArray) {
            FindObject(game, "JoinWrapper", "JoinButton")?.GetComponent<Button>().SetInteractable(true);
        }
    }

    public static void Init(ref GameObject bingoGameBrowser) {
        _fetchText = GetGameObjectChild(bingoGameBrowser, "FetchText");
        _back = GetGameObjectChild(bingoGameBrowser, "Back");

        _back?.GetComponent<Button>().onClick.AddListener(
            delegate {
                BingoEncapsulator.BingoGameBrowser?.SetActive(false);
                BingoEncapsulator.BingoMenu?.SetActive(true);
                Main.NetworkManager.SetState(Types.State.InMenu);
            }
        );

        _gameListWrapper = GetGameObjectChild(bingoGameBrowser, "GameList");
        _gameList = GetGameObjectChild(GetGameObjectChild(_gameListWrapper, "Viewport"), "Content");
        _gameTemplate = GetGameObjectChild(_gameList, "GameTemplate");
    }

    private static void ClearOldGames() {
        if (_gameList != null)
            foreach (Transform child in _gameList.transform)
                if (child.gameObject.name != "GameTemplate")
                    Object.Destroy(child.gameObject);

        GameArray.Clear();
    }

    public static void PopulateGames(List<PublicGameData> games) {
        var difficultyNames = new Dictionary<int, string> {
            { 0, "HARMLESS" },
            { 1, "LENIENT" },
            { 2, "STANDARD" },
            { 3, "VIOLENT" },
            { 4, "BRUTAL" }
        };

        if (games.Count == 0) {
            _fetchText?.GetComponent<TextMeshProUGUI>().SetText("No public games currently available.");
            return;
        }

        //Clear previous games
        ClearOldGames();

        foreach (var game in games) {
            var gameBar = Object.Instantiate(_gameTemplate, _gameTemplate?.transform.parent);
            
            GetGameObjectChild(gameBar, "HostName")?.GetComponent<Text>().SetText(game.HostUsername);
            GetGameObjectChild(gameBar, "Difficulty")?.GetComponent<Text>().SetText(difficultyNames[game.Difficulty]);
            GetGameObjectChild(gameBar, "Players")?.GetComponent<Text>()
                .SetText($"{game.CurrentPlayers}/{game.MaxPlayers}");

            FindObject(gameBar, "JoinWrapper", "JoinButton")?.GetComponent<Button>().onClick
                .AddListener(
                    delegate {
                        LockUI();
                        BingoMenuController.JoinRoom(game.Password);
                    }
                );

            gameBar?.SetActive(true);

            if (gameBar != null) GameArray.Add(gameBar);
        }

        _fetchText?.SetActive(false);
        _gameListWrapper?.SetActive(true);
        _gameList?.SetActive(true);
        fetchDone = true;
    }

    public static void DisplayError() {
        _fetchText?.GetComponent<TextMeshProUGUI>().SetText("Unable to connect to server.");
    }

    public static void FetchGames() {
        _fetchText?.SetActive(true);
        _gameListWrapper?.SetActive(false);
        _gameList?.SetActive(false);
        _fetchText?.GetComponent<TextMeshProUGUI>().SetText("Fetching games, please wait...");
        fetchDone = false;

        Requests.RequestGames();
    }
}