﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UltraBINGO.NetworkMessages;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public class BingoSetTeamsMenu {
    public static Dictionary<int, Color> teamColors = new() {
        { 0, new Color(1, 1, 1, 1) },
        { 1, new Color(1, 0, 0, 1) },
        { 2, new Color(0, 1, 0, 1) },
        { 3, new Color(0, 0, 1, 1) },
        { 4, new Color(1, 1, 0, 1) }
    };

    public static GameObject Root;

    public static GameObject PlayerGrid;
    public static GameObject ButtonTemplate;
    public static GameObject CancelButton;
    public static GameObject ResetButton;
    public static GameObject FinishButton;
    public static GameObject TeamSelectionPanel;
    public static GameObject TeamSelectionBackButton;

    public static GameObject currentPlayerObject;

    public static List<GameObject> TeamSelectionPanelButtons = [];
    public static Dictionary<string, int> currentTeamChanges = new();

    public static int playersMapped;
    public static int playersToMap;

    public static void ReturnToLobbyMenu() {
        BingoEncapsulator.BingoSetTeams.SetActive(false);
        BingoEncapsulator.BingoLobbyScreen.SetActive(true);
    }

    public static void Cancel() {
        currentTeamChanges.Clear();
        foreach (var go in TeamSelectionPanelButtons) go.SetActive(true);
        ReturnToLobbyMenu();
    }

    public static void Discard() {
        var cts = new ClearTeamSettings {
            gameId = GameManager.CurrentGame.gameId,
            ticket = NetworkManager.CreateRegisterTicket()
        };

        NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(cts));
        ReturnToLobbyMenu();
    }

    public static void Submit() {
        if (playersToMap != playersMapped) {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                "One or more players have not been assigned to a team.");
            return;
        }

        var ts = new TeamSettings {
            gameId = GameManager.CurrentGame.gameId,
            teams = currentTeamChanges,
            ticket = NetworkManager.CreateRegisterTicket()
        };

        NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(ts));
        ReturnToLobbyMenu();
    }

    public static void PrepareChanges() {
        var playerList = GameManager.CurrentGame.currentPlayers;
        var playerSteamIds = playerList.Keys.ToList();
        foreach (var id in playerSteamIds) currentTeamChanges[id] = 0;
    }

    public static void OpenTeamColorPanel(ref GameObject player, string playerSteamId, string playerName) {
        if (currentTeamChanges.ContainsKey(playerSteamId)) {
            currentPlayerObject = player;
            GetGameObjectChild(TeamSelectionPanel, "PlayerName").GetComponent<TextMeshProUGUI>().text =
                "<color=orange>" + playerName + "</color>";
            TeamSelectionPanel.SetActive(true);
        } else {
            Logging.Warn("Tried to update team for SteamID " + playerSteamId + " but it's not set in the dict!");
        }
    }

    public static void updatePlayerTeam(int teamId) {
        if (currentTeamChanges[currentPlayerObject.name] == 0) playersMapped++;
        currentTeamChanges[currentPlayerObject.name] = teamId;
        currentPlayerObject.GetComponent<Image>().color = teamColors[teamId];
        TeamSelectionPanel.SetActive(false);
    }

    public static void Setup() {
        var playerList = GameManager.CurrentGame.currentPlayers;
        playersToMap = playerList.Count;
        playersMapped = 0;

        var playerSteamIds = playerList.Keys.ToList();

        //Start by clearing out the existing player buttons.
        foreach (Transform child in PlayerGrid.transform)
            if (child.gameObject.name != "PlayerTemplate")
                Object.Destroy(child.gameObject);

        //Reset the current hot changes.
        currentTeamChanges.Clear();

        //Then create buttons for each player current connected to the game.
        foreach (var id in playerSteamIds) {
            var playerName = GameManager.CurrentGame.currentPlayers[id].username;
            var playerTeamButton = Object.Instantiate(ButtonTemplate, PlayerGrid.transform);
            playerTeamButton.name = id;
            GetGameObjectChild(playerTeamButton, "Text").GetComponent<TextMeshProUGUI>().text = playerName;

            playerTeamButton.GetComponent<Button>().onClick.AddListener(delegate {
                OpenTeamColorPanel(ref playerTeamButton, id, playerName);
            });
            playerTeamButton.SetActive(true);
        }

        //If there are only 2/3 teams, disable the excess buttons.
        var maxTeams = GameManager.CurrentGame.gameSettings.maxTeams;
        for (var x = maxTeams; x < TeamSelectionPanelButtons.Count; x++) TeamSelectionPanelButtons[x].SetActive(false);

        //And prepare the hot changes.
        PrepareChanges();
    }

    public static void Init(ref GameObject BingoSetTeams) {
        if (ButtonTemplate == null) ButtonTemplate = new GameObject();

        PlayerGrid = GetGameObjectChild(BingoSetTeams, "PlayerContainer");

        //Have to create the button with normal Text instead of TextMeshProUGUI as trying to instantiate an object with the latter component causes crashes.
        ButtonTemplate = GetGameObjectChild(PlayerGrid, "PlayerTemplate");
        ButtonTemplate.SetActive(false);

        TeamSelectionPanel = GetGameObjectChild(BingoSetTeams, "TeamSelection");

        var TeamSelectionPanelSub = GetGameObjectChild(TeamSelectionPanel, "TeamColorContainer");

        TeamSelectionPanelButtons.Clear(); //Remove previously destroyed references
        TeamSelectionPanelButtons.Add(GetGameObjectChild(TeamSelectionPanelSub, "Red"));
        TeamSelectionPanelButtons.Add(GetGameObjectChild(TeamSelectionPanelSub, "Green"));
        TeamSelectionPanelButtons.Add(GetGameObjectChild(TeamSelectionPanelSub, "Blue"));
        TeamSelectionPanelButtons.Add(GetGameObjectChild(TeamSelectionPanelSub, "Yellow"));

        GetGameObjectChild(TeamSelectionPanelSub, "Red").GetComponent<Button>().onClick
            .AddListener(delegate { updatePlayerTeam(1); });
        GetGameObjectChild(TeamSelectionPanelSub, "Green").GetComponent<Button>().onClick
            .AddListener(delegate { updatePlayerTeam(2); });
        GetGameObjectChild(TeamSelectionPanelSub, "Blue").GetComponent<Button>().onClick
            .AddListener(delegate { updatePlayerTeam(3); });
        GetGameObjectChild(TeamSelectionPanelSub, "Yellow").GetComponent<Button>().onClick
            .AddListener(delegate { updatePlayerTeam(4); });

        TeamSelectionBackButton = GetGameObjectChild(TeamSelectionPanel, "Back");
        TeamSelectionBackButton.GetComponent<Button>().onClick
            .AddListener(delegate { TeamSelectionPanel.SetActive(false); });

        CancelButton = GetGameObjectChild(BingoSetTeams, "Cancel");
        CancelButton.GetComponent<Button>().onClick.AddListener(delegate { Cancel(); });

        ResetButton = GetGameObjectChild(BingoSetTeams, "Reset");
        ResetButton.GetComponent<Button>().onClick.AddListener(delegate { Discard(); });

        FinishButton = GetGameObjectChild(BingoSetTeams, "Finish");
        FinishButton.GetComponent<Button>().onClick.AddListener(delegate { Submit(); });
    }
}