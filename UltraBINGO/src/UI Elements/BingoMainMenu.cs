using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoMainMenu {
    public static GameObject? Root;
    public static GameObject? HostGame;
    public static GameObject? JoinGame;
    public static GameObject? JoinGameInput;
    public static GameObject? GameBrowser;
    public static GameObject? Back;

    public static GameObject? MapCheck;
    public static GameObject? MapWarn;
    public static GameObject? MissingMapsList;

    public static GameObject? DiscordButton;
    public static GameObject? VersionInfo;

    public static GameObject? MOTDContainer;
    public static string MOTD = "";

    public static GameObject? RankSelection;
    public static List<string>? ranks;


    public static void LockUI() {
        HostGame.GetComponent<Button>().interactable = false;
        JoinGame.GetComponent<Button>().interactable = false;
        GameBrowser.GetComponent<Button>().interactable = false;
    }

    public static void UnlockUI() {
        HostGame.GetComponent<Button>().interactable = true;
        JoinGame.GetComponent<Button>().interactable = true;
        GameBrowser.GetComponent<Button>().interactable = true;
    }

    public static GameObject Init(ref GameObject BingoMenu) {
        HostGame = GetGameObjectChild(BingoMenu, "Host Game");
        HostGame.GetComponent<Button>().onClick.AddListener(delegate {
            LockUI();
            BingoMenuController.CreateRoom();
        });

        JoinGame = GetGameObjectChild(BingoMenu, "Join Game");
        JoinGame.GetComponent<Button>().onClick.AddListener(delegate {
            LockUI();
            var input = GetGameObjectChild(JoinGameInput, "InputField (TMP)");

            var password = input.GetComponent<TMP_InputField>().text;
            BingoMenuController.JoinRoom(password);
        });

        JoinGameInput = GetGameObjectChild(JoinGame, "IdInput");

        GameBrowser = GetGameObjectChild(BingoMenu, "Match Browser");
        GameBrowser.GetComponent<Button>().onClick.AddListener(delegate {
            BingoEncapsulator.BingoMenu.SetActive(false);
            BingoEncapsulator.BingoGameBrowser.SetActive(true);
            NetworkManager.SetState(State.INBROWSER);
            BingoBrowser.FetchGames();
        });

        MapCheck = GetGameObjectChild(BingoMenu, "MapCheck");
        MapCheck.GetComponent<Button>().onClick.AddListener(delegate { MapWarn.SetActive(true); });

        MapWarn = GetGameObjectChild(BingoMenu, "MapWarn");
        MapWarn.SetActive(false);
        MissingMapsList = GetGameObjectChild(GetGameObjectChild(MapWarn, "Panel"), "MissingMapList");

        Back = GetGameObjectChild(BingoMenu, "Back");
        Back.GetComponent<Button>().onClick.AddListener(delegate { BingoMenuController.ReturnToMenu(); });

        DiscordButton = GetGameObjectChild(BingoMenu, "Discord");
        DiscordButton.GetComponent<Button>().onClick.AddListener(delegate {
            Application.OpenURL("https://discord.gg/VyzFJwEWtJ");
        });
        VersionInfo = GetGameObjectChild(BingoMenu, "Version");

        MOTDContainer = GetGameObjectChild(BingoMenu, "MOTD");
        GetGameObjectChild(MOTDContainer, "Content").GetComponent<TextMeshProUGUI>().text = MOTD;

        RankSelection = GetGameObjectChild(BingoMenu, "RankSelection");
        TMP_Dropdown rankSelector;
        rankSelector = GetGameObjectChild(RankSelection, "Dropdown").GetComponent<TMP_Dropdown>();
        if (ranks != null) {
            rankSelector.ClearOptions();
            rankSelector.AddOptions(ranks);
        }


        rankSelector.onValueChanged.AddListener(delegate(int _) {
            NetworkManager.requestedRank = rankSelector.options[rankSelector.value].text;
            NetworkManager.lastRankUsedConfig.Value = NetworkManager.requestedRank;
        });

        return Root;
    }
}