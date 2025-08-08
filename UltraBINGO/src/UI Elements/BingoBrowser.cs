using System.Collections.Generic;
using TMPro;
using UltraBINGO.NetworkMessages;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoBrowser {
    public static GameObject Root;
    public static GameObject FetchText;
    public static GameObject GameTemplate;
    public static GameObject GameListWrapper;
    public static GameObject GameList;
    public static GameObject Back;

    public static bool fetchDone;

    public static List<GameObject> gameArray = [];

    public static void LockUI() {
        //Lock all the join buttons of any games in the browser.

        foreach (var game in gameArray)
            GetGameObjectChild(GetGameObjectChild(game, "JoinWrapper"), "JoinButton").GetComponent<Button>()
                .interactable = false;
    }

    public static void UnlockUI() {
        //Unlock all the join buttons of any games in the browser.
        foreach (var game in gameArray)
            GetGameObjectChild(GetGameObjectChild(game, "JoinWrapper"), "JoinButton").GetComponent<Button>()
                .interactable = true;
    }

    public static void Init(ref GameObject BingoGameBrowser) {
        FetchText = GetGameObjectChild(BingoGameBrowser, "FetchText");
        Back = GetGameObjectChild(BingoGameBrowser, "Back");
        Back.GetComponent<Button>().onClick.AddListener(delegate {
            BingoEncapsulator.BingoGameBrowser.SetActive(false);
            BingoEncapsulator.BingoMenu.SetActive(true);
            NetworkManager.SetState(State.INMENU);
        });

        GameListWrapper = GetGameObjectChild(BingoGameBrowser, "GameList");
        GameList = GetGameObjectChild(GetGameObjectChild(GameListWrapper, "Viewport"), "Content");
        GameTemplate = GetGameObjectChild(GameList, "GameTemplate");
    }

    public static void clearOldGames() {
        foreach (Transform child in GameList.transform)
            if (child.gameObject.name != "GameTemplate")
                Object.Destroy(child.gameObject);

        gameArray.Clear();
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
            FetchText.GetComponent<TextMeshProUGUI>().text = "No public games currently available.";
            return;
        }

        //Clear previous games
        clearOldGames();

        foreach (var game in games) {
            var gameBar = Object.Instantiate(GameTemplate, GameTemplate.transform.parent);
            GetGameObjectChild(gameBar, "HostName").GetComponent<Text>().text = game.C_USERNAME;
            GetGameObjectChild(gameBar, "Difficulty").GetComponent<Text>().text = difficultyNames[game.R_DIFFICULTY];
            GetGameObjectChild(gameBar, "Players").GetComponent<Text>().text =
                game.R_CURRENTPLAYERS + "/" + game.R_MAXPLAYERS;
            GetGameObjectChild(GetGameObjectChild(gameBar, "JoinWrapper"), "JoinButton").GetComponent<Button>().onClick
                .AddListener(delegate {
                    LockUI();
                    BingoMenuController.JoinRoom(game.R_PASSWORD);
                });
            gameBar.SetActive(true);
            gameArray.Add(gameBar);
        }

        FetchText.SetActive(false);
        GameListWrapper.SetActive(true);
        GameList.SetActive(true);
        fetchDone = true;
    }

    public static void DisplayError() {
        FetchText.GetComponent<TextMeshProUGUI>().text = "Unable to connect to server.";
    }

    public static void FetchGames() {
        FetchText.SetActive(true);
        GameListWrapper.SetActive(false);
        GameList.SetActive(false);
        FetchText.GetComponent<TextMeshProUGUI>().text = "Fetching games, please wait...";
        fetchDone = false;
        NetworkManager.RequestGames();
    }
}