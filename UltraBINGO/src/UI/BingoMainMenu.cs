using System.Collections.Generic;
using TMPro;
using UltraBINGO.Net;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.UI;

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

    public static GameObject? motdContainer;
    public static string motd = "";

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

    public static GameObject Init(ref GameObject bingoMenu) {
        HostGame = GetGameObjectChild(bingoMenu, "Host Game");
        HostGame.GetComponent<Button>().onClick.AddListener(
            delegate {
                LockUI();
                BingoMenuController.CreateRoom();
            }
        );

        JoinGame = GetGameObjectChild(bingoMenu, "Join Game");
        JoinGame.GetComponent<Button>().onClick.AddListener(
            delegate {
                LockUI();
                var input = GetGameObjectChild(JoinGameInput, "InputField (TMP)");

                var password = input.GetComponent<TMP_InputField>().text;
                BingoMenuController.JoinRoom(password);
            }
        );

        JoinGameInput = GetGameObjectChild(JoinGame, "IdInput");

        GameBrowser = GetGameObjectChild(bingoMenu, "Match Browser");
        GameBrowser.GetComponent<Button>().onClick.AddListener(
            delegate {
                BingoEncapsulator.BingoMenu.SetActive(false);
                BingoEncapsulator.BingoGameBrowser.SetActive(true);
                Main.NetworkManager.SetState(Types.State.InBrowser);
                BingoBrowser.FetchGames();
            }
        );

        MapCheck = GetGameObjectChild(bingoMenu, "MapCheck");
        MapCheck.GetComponent<Button>().onClick.AddListener(delegate { MapWarn.SetActive(true); });

        MapWarn = GetGameObjectChild(bingoMenu, "MapWarn");
        MapWarn.SetActive(false);
        MissingMapsList = GetGameObjectChild(GetGameObjectChild(MapWarn, "Panel"), "MissingMapList");

        Back = GetGameObjectChild(bingoMenu, "Back");
        Back.GetComponent<Button>().onClick.AddListener(delegate { BingoMenuController.ReturnToMenu(); });

        DiscordButton = GetGameObjectChild(bingoMenu, "Discord");
        DiscordButton.GetComponent<Button>().onClick
            .AddListener(delegate { Application.OpenURL("https://discord.gg/VyzFJwEWtJ"); });
        VersionInfo = GetGameObjectChild(bingoMenu, "Version");

        motdContainer = GetGameObjectChild(bingoMenu, "MOTD");
        GetGameObjectChild(motdContainer, "Content").GetComponent<TextMeshProUGUI>().text = motd;

        RankSelection = GetGameObjectChild(bingoMenu, "RankSelection");
        TMP_Dropdown rankSelector;
        rankSelector = GetGameObjectChild(RankSelection, "Dropdown").GetComponent<TMP_Dropdown>();
        if (ranks != null) {
            rankSelector.ClearOptions();
            rankSelector.AddOptions(ranks);
        }


        rankSelector.onValueChanged.AddListener(
            delegate(int _) {
                GameManager.RequestedRank = rankSelector.options[rankSelector.value].text;
                Main.ModConfig.LastRankUsed.Value = GameManager.RequestedRank;
            }
        );

        return Root;
    }
}