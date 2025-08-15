using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UltraBINGO.Net;
using UltraBINGO.Packets;
using UltraBINGO.Util;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI;

public static class BingoSetTeamsMenu {
    private static readonly Dictionary<int, Color> TeamColors = new() {
        { 0, new Color(1, 1, 1, 1) },
        { 1, new Color(1, 0, 0, 1) },
        { 2, new Color(0, 1, 0, 1) },
        { 3, new Color(0, 0, 1, 1) },
        { 4, new Color(1, 1, 0, 1) }
    };

    private static GameObject? _playerGrid;
    private static GameObject? _buttonTemplate;
    private static GameObject? _cancelButton;
    private static GameObject? _resetButton;
    private static GameObject? _finishButton;
    private static GameObject? _teamSelectionPanel;
    private static GameObject? _teamSelectionBackButton;
    private static GameObject? _currentPlayerObject;

    private static readonly List<GameObject?> TeamSelectionPanelButtons = [];
    private static readonly Dictionary<string, int> CurrentTeamChanges = new();

    private static int _playersMapped;
    private static int _playersToMap;

    private static void ReturnToLobbyMenu() {
        BingoEncapsulator.BingoSetTeams?.SetActive(false);
        BingoEncapsulator.BingoLobbyScreen?.SetActive(true);
    }

    private static void Cancel() {
        CurrentTeamChanges.Clear();
        
        foreach (var go in TeamSelectionPanelButtons) go?.SetActive(true);
        
        ReturnToLobbyMenu();
    }

    private static async Task Discard() {
        await NetworkManager.SendEncodedMessage(
            new ClearTeamSettings {
                GameId = GameManager.CurrentGame.GameId,
                Ticket = NetworkManager.CreateRegisterTicket()
            }
        );
        ReturnToLobbyMenu();
    }

    private static async Task Submit() {
        if (_playersToMap != _playersMapped) {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                "One or more players have not been assigned to a team."
            );
            return;
        }

        await NetworkManager.SendEncodedMessage(
            new TeamSettings {
                GameId = GameManager.CurrentGame.GameId,
                Teams = CurrentTeamChanges,
                Ticket = NetworkManager.CreateRegisterTicket()
            }
        );

        ReturnToLobbyMenu();
    }

    private static void PrepareChanges() {
        var playerList = GameManager.CurrentGame.CurrentPlayers;
        var playerSteamIds = playerList.Keys.ToList();

        foreach (var id in playerSteamIds) CurrentTeamChanges[id] = 0;
    }

    private static void OpenTeamColorPanel(ref GameObject player, string playerSteamId, string playerName) {
        if (CurrentTeamChanges.ContainsKey(playerSteamId)) {
            _currentPlayerObject = player;

            var text = GetGameObjectChild(_teamSelectionPanel, "PlayerName")?.GetComponent<TextMeshProUGUI>();

            if (text != null)
                text.text = $"<color=orange>{playerName}</color>";

            _teamSelectionPanel?.SetActive(true);
        } else {
            Logging.Warn($"Tried to update team for SteamID {playerSteamId} but it's not set in the dict!");
        }
    }

    private static void UpdatePlayerTeam(int teamId) {
        if (_currentPlayerObject == null) return;

        if (CurrentTeamChanges[_currentPlayerObject.name] == 0) _playersMapped++;

        CurrentTeamChanges[_currentPlayerObject.name] = teamId;
        _currentPlayerObject.GetComponent<Image>().color = TeamColors[teamId];
        _teamSelectionPanel?.SetActive(false);
    }

    public static void Setup() {
        var playerList = GameManager.CurrentGame.CurrentPlayers;
        _playersToMap = playerList.Count;
        _playersMapped = 0;

        var playerSteamIds = playerList.Keys.ToList();

        //Start by clearing out the existing player buttons.
        if (_playerGrid != null) {
            foreach (Transform child in _playerGrid.transform)
                if (child.gameObject.name != "PlayerTemplate")
                    Object.Destroy(child.gameObject);

            //Reset the current hot changes.
            CurrentTeamChanges.Clear();

            //Then create buttons for each player current connected to the game.
            foreach (var id in playerSteamIds) {
                var playerName = GameManager.CurrentGame.CurrentPlayers[id].Username;
                var playerTeamButton = Object.Instantiate(_buttonTemplate, _playerGrid.transform);

                if (playerTeamButton == null) continue;

                playerTeamButton.name = id;

                var text = GetGameObjectChild(playerTeamButton, "Text");

                if (text != null) text.GetComponent<TextMeshProUGUI>().text = playerName;

                playerTeamButton.GetComponent<Button>().onClick.AddListener(
                    delegate { OpenTeamColorPanel(ref playerTeamButton, id, playerName); }
                );

                playerTeamButton.SetActive(true);
            }
        }

        // If there are only 2/3 teams, disable the excess buttons.
        var maxTeams = GameManager.CurrentGame.GameSettings.MaxTeams;

        for (var x = maxTeams; x < TeamSelectionPanelButtons.Count; x++)
            TeamSelectionPanelButtons[x]?.SetActive(false);

        // And prepare the hot changes.
        PrepareChanges();
    }

    public static void Init(ref GameObject bingoSetTeams) {
        if (_buttonTemplate == null) _buttonTemplate = new GameObject();

        _playerGrid = GetGameObjectChild(bingoSetTeams, "PlayerContainer");

        // Have to create the button with normal Text instead of TextMeshProUGUI as trying to instantiate an object with the latter component causes crashes.
        _buttonTemplate = GetGameObjectChild(_playerGrid, "PlayerTemplate");
        _buttonTemplate?.SetActive(false);

        _teamSelectionPanel = GetGameObjectChild(bingoSetTeams, "TeamSelection");

        var teamSelectionPanelSub = GetGameObjectChild(_teamSelectionPanel, "TeamColorContainer");

        TeamSelectionPanelButtons.Clear(); //Remove previously destroyed references
        TeamSelectionPanelButtons.Add(GetGameObjectChild(teamSelectionPanelSub, "Red"));
        TeamSelectionPanelButtons.Add(GetGameObjectChild(teamSelectionPanelSub, "Green"));
        TeamSelectionPanelButtons.Add(GetGameObjectChild(teamSelectionPanelSub, "Blue"));
        TeamSelectionPanelButtons.Add(GetGameObjectChild(teamSelectionPanelSub, "Yellow"));

        GetGameObjectChild(teamSelectionPanelSub, "Red")?.GetComponent<Button>().onClick
            .AddListener(delegate { UpdatePlayerTeam(1); });

        GetGameObjectChild(teamSelectionPanelSub, "Green")?.GetComponent<Button>().onClick
            .AddListener(delegate { UpdatePlayerTeam(2); });

        GetGameObjectChild(teamSelectionPanelSub, "Blue")?.GetComponent<Button>().onClick
            .AddListener(delegate { UpdatePlayerTeam(3); });

        GetGameObjectChild(teamSelectionPanelSub, "Yellow")?.GetComponent<Button>().onClick
            .AddListener(delegate { UpdatePlayerTeam(4); });

        _teamSelectionBackButton = GetGameObjectChild(_teamSelectionPanel, "Back");

        _teamSelectionBackButton?.GetComponent<Button>().onClick
            .AddListener(delegate { _teamSelectionPanel?.SetActive(false); });

        _cancelButton = GetGameObjectChild(bingoSetTeams, "Cancel");
        _cancelButton?.GetComponent<Button>().onClick.AddListener(Cancel);

        _resetButton = GetGameObjectChild(bingoSetTeams, "Reset");
        _resetButton?.GetComponent<Button>().onClick.AddListener(delegate { Discard().Wait(); });

        _finishButton = GetGameObjectChild(bingoSetTeams, "Finish");
        _finishButton?.GetComponent<Button>().onClick.AddListener(delegate { Submit().Wait(); });
    }
}