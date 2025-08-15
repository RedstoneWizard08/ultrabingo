using System.Collections.Generic;
using TMPro;
using UltraBINGO.Net;
using UltraBINGO.Util;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.UI;

public static class BingoMainMenu {
    private static GameObject? HostGame;
    private static GameObject? JoinGame;
    private static GameObject? JoinGameInput;
    private static GameObject? GameBrowser;
    private static GameObject? Back;

    private static GameObject? MapCheck;
    private static GameObject? MapWarn;
    public static GameObject? MissingMapsList;

    private static GameObject? DiscordButton;
    public static GameObject? VersionInfo;

    public static GameObject? motdContainer;
    public static string motd = "";

    public static GameObject? RankSelection;
    public static List<string>? ranks;

    private static void LockUI() {
        if (HostGame != null) HostGame.GetComponent<Button>().interactable = false;
        if (JoinGame != null) JoinGame.GetComponent<Button>().interactable = false;
        if (GameBrowser != null) GameBrowser.GetComponent<Button>().interactable = false;
    }

    public static void UnlockUI() {
        if (HostGame != null) HostGame.GetComponent<Button>().interactable = true;
        if (JoinGame != null) JoinGame.GetComponent<Button>().interactable = true;
        if (GameBrowser != null) GameBrowser.GetComponent<Button>().interactable = true;
    }

    public static void Init(ref GameObject bingoMenu) {
        HostGame = GetGameObjectChild(bingoMenu, "Host Game");
        HostGame?.GetComponent<Button>().onClick.AddListener(
            delegate {
                LockUI();
                BingoMenuController.CreateRoom();
            }
        );

        JoinGame = GetGameObjectChild(bingoMenu, "Join Game");
        JoinGame?.GetComponent<Button>().onClick.AddListener(
            delegate {
                LockUI();
                var input = GetGameObjectChild(JoinGameInput, "InputField (TMP)");
                var password = input?.GetComponent<TMP_InputField>().text;

                if (password != null) BingoMenuController.JoinRoom(password);
            }
        );

        JoinGameInput = GetGameObjectChild(JoinGame, "IdInput");

        GameBrowser = GetGameObjectChild(bingoMenu, "Match Browser");
        GameBrowser?.GetComponent<Button>().onClick.AddListener(
            delegate {
                BingoEncapsulator.BingoMenu?.SetActive(false);
                BingoEncapsulator.BingoGameBrowser?.SetActive(true);
                Main.NetworkManager.SetState(Types.State.InBrowser);
                BingoBrowser.FetchGames();
            }
        );

        MapCheck = FindObject(bingoMenu, "MapCheck");
        MapCheck?.GetComponent<Button>().onClick.AddListener(delegate { MapWarn?.SetActive(true); });

        MapWarn = FindObject(bingoMenu, "MapWarn");
        MapWarn?.SetActive(false);
        MissingMapsList = FindObject(MapWarn, "Panel", "MissingMapList");

        Back = FindObject(bingoMenu, "Back");
        Back?.GetComponent<Button>().onClick.AddListener(BingoMenuController.ReturnToMenu);

        DiscordButton = FindObject(bingoMenu, "Discord");
        DiscordButton?.GetComponent<Button>().onClick.AddListener(Misc.OpenDiscordLink);
        
        VersionInfo = FindObject(bingoMenu, "Version");
        motdContainer = FindObject(bingoMenu, "MOTD");

        FindObject(motdContainer, "Content")?.GetComponent<TextMeshProUGUI>().SetText(motd);

        RankSelection = FindObject(bingoMenu, "RankSelection");

        var rankSelector = FindObject(RankSelection, "Dropdown")?.GetComponent<TMP_Dropdown>();

        if (ranks != null) {
            rankSelector?.ClearOptions();
            rankSelector?.AddOptions(ranks);
        }

        rankSelector?.onValueChanged.AddListener(
            delegate {
                GameManager.RequestedRank = rankSelector.options[rankSelector.value].text;
                Main.ModConfig.LastRankUsed.Value = GameManager.RequestedRank;
            }
        );
    }
}