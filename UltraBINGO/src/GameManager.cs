using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UltraBINGO.Components;
using UltraBINGO.Net;
using UltraBINGO.Packets;
using UltraBINGO.Types;
using UltraBINGO.UI;
using UltraBINGO.Util;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;
using Object = UnityEngine.Object;

namespace UltraBINGO;

public static class GameManager {
    public static Game CurrentGame {
        get => currentSetGame ?? throw new NullReferenceException("Current game was not set!");
        private set => currentSetGame = value;
    }

    public static Game? currentSetGame;
    public static bool IsInBingoLevel;
    public static int CurrentRow;
    public static int CurrentColumn;
    public static string CurrentTeam = "";
    public static List<string>? Teammates;
    public static bool HasSent;
    public static bool EnteringAngryLevel;
    public static bool TriedToActivateCheats;
    public static bool IsDownloadingLevel = false;
    public static bool IsSwitchingLevels;
    public static bool alreadyStartedVote;
    public static float dominationTimer = 0;
    public static bool hasRankAccess = false;
    public static VoteData? voteData = new(false);

    public static async Task SwapRerolledMap(string oldMapId, GameLevel level, int column, int row) {
        if (IsInBingoLevel) {
            CurrentGame.Grid.LevelTable[$"{column}-{row}"] = level;
            var a = GetGameObjectChild(BingoCardPauseMenu.Grid, $"{column}-{row}")?.GetComponent<BingoLevelData>();

            if (a != null) {
                a.IsAngryLevel = level.IsAngryLevel;
                a.AngryParentBundle = level.AngryParentBundle;
                a.AngryLevelId = level.AngryLevelId;
                a.LevelName = level.LevelName;
            }
        }

        if (oldMapId != GetSceneName()) return;

        Logging.Message("Currently on old map - switching in 5 seconds");
        await Task.Delay(5000);
        var b = GetGameObjectChild(BingoCardPauseMenu.Grid, $"{column}-{row}")?.GetComponent<Button>();
        b?.onClick.Invoke();
    }

    public static void UpdateGridPosition(int row, int column) {
        CurrentRow = row;
        CurrentColumn = column;
    }

    public static void ClearGameVariables() {
        currentSetGame = null;
        CurrentTeam = "";
        CurrentRow = 0;
        CurrentColumn = 0;
        IsInBingoLevel = false;
        Teammates = null;
        voteData = null;

        BingoMapSelection.ClearList();

        //Cleanup the bingo grid if on the main menu.
        if (GetSceneName() == "Main Menu") BingoCard.Cleanup();
    }

    public static async Task HumiliateSelf() {
        var ca = new CheatActivation {
            Username = SanitiseUsername(Steamworks.SteamClient.Name),
            GameId = CurrentGame.GameId,
            SteamId = Steamworks.SteamClient.SteamId.ToString()
        };

        await NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(ca));
    }

    public static async Task LeaveGame(bool isInLevel = false) {
        //Send a request to the server saying we want to leave.
        await NetworkManager.SendLeaveGameRequest(CurrentGame.GameId);

        //When that's sent off, close the connection on our end.
        Logging.Message("Closing connection");
        NetworkManager.DisconnectWebSocket(1000, "Normal close");

        ClearGameVariables();

        if (!isInLevel) {
            //If dc'ing from lobby/card/end screen, return to the bingo menu.
            BingoEncapsulator.BingoCardScreen?.SetActive(false);
            BingoEncapsulator.BingoLobbyScreen?.SetActive(false);
            BingoEncapsulator.BingoEndScreen?.SetActive(false);
            BingoEncapsulator.BingoMenu?.SetActive(true);

            NetworkManager.SetState(Types.State.InMenu);
        } else {
            NetworkManager.SetState(Types.State.Normal);
        }
    }

    public static void MoveToCard(int gameType) {
        BingoCard.UpdateTitles(gameType);
        BingoLobby.UnlockUI();

        BingoEncapsulator.BingoLobbyScreen?.SetActive(false);
        BingoEncapsulator.BingoCardScreen?.SetActive(true);
    }

    //Check if we're the host of the game we're in.
    public static bool PlayerIsHost() {
        return Steamworks.SteamClient.SteamId.ToString() == CurrentGame.GameHost;
    }

    //Check if the amount of selected maps is enough to fill the grid.
    public static bool PreStartChecks() {
        var gridSize = CurrentGame.GameSettings.GridSize;
        var requiredMaps = gridSize * gridSize;

        if (BingoMapSelection.NumOfMapsTotal < requiredMaps) {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                $"Not enough maps selected. Add more map pools, or reduce the grid size.\n(<color=orange>{requiredMaps} </color>required, <color=orange>{BingoMapSelection.NumOfMapsTotal}</color> selected)");
            return false;
        }

        if (CurrentGame.GameSettings.TeamComposition == 1 && CurrentGame.GameSettings.HasManuallySetTeams == false) {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Teams must be set before starting the game.");
            return false;
        }

        return true;
    }

    //Reset temporary vars on level change.
    public static void ResetVars() {
        HasSent = false;
        EnteringAngryLevel = false;
        TriedToActivateCheats = false;
        IsSwitchingLevels = false;
    }

    //Update displayed player list when a player joins/leaves game.
    public static void RefreshPlayerList() {
        try {
            if (GetSceneName() != "Main Menu") return;

            BingoLobby.PlayerList?.SetActive(false);

            var playerList = GetGameObjectChild(BingoLobby.PlayerList, "PlayerList");
            var playerTemplate = GetGameObjectChild(playerList, "PlayerTemplate");

            if (playerList != null) {
                foreach (Transform child in playerList.transform)
                    if (child.gameObject.name != "PlayerTemplate")
                        Object.Destroy(child.gameObject);

                foreach (var steamId in CurrentGame.CurrentPlayers.Keys.ToList()) {
                    var player = Object.Instantiate(playerTemplate, playerList?.transform);

                    var hostStripped = "";
                    //If the host name has color tags in their Steam name, strip them.
                    if (steamId == CurrentGame.GameHost)
                        hostStripped =
                            $"<color=orange>{Regex.Replace(CurrentGame.CurrentPlayers[steamId].Username, @"^<[^>]*>", "")}</color>";

                    var name = GetGameObjectChild(player, "PlayerName");

                    if (name != null)
                        name.GetComponent<Text>().text =
                            (steamId == CurrentGame.GameHost
                                ? hostStripped
                                : CurrentGame.CurrentPlayers[steamId].Username)
                            + (CurrentGame.CurrentPlayers[steamId].Rank != ""
                                ? $" | {CurrentGame.CurrentPlayers[steamId].Rank}"
                                : "");

                    var kick = GetGameObjectChild(player, "Kick");

                    if (kick != null) {
                        kick.GetComponent<Button>().onClick.AddListener(delegate {
                            NetworkManager.KickPlayer(steamId).Wait();
                        });
                        kick.transform.localScale = Vector3.one;
                        kick.SetActive(Steamworks.SteamClient.SteamId.ToString() == CurrentGame.GameHost &&
                                       steamId != Steamworks.SteamClient.SteamId.ToString());
                    }

                    player?.SetActive(true);
                }
            }

            BingoLobby.PlayerList?.SetActive(true);
        } catch (Exception e) {
            Logging.Error("Something went wrong when trying to update player list");
            Logging.Error(e.ToString());
        }
    }

    //Setup the bingo grid.
    public static void SetupBingoCardDynamic() {
        //Resize the GridLayoutGroup based on the grid size.
        var gridObj = GetGameObjectChild(BingoCard.Root, "BingoGrid");
        if (gridObj != null) {
            gridObj.GetComponent<GridLayoutGroup>().constraintCount = CurrentGame.Grid.Size;
            gridObj.GetComponent<GridLayoutGroup>().spacing = new Vector2(30, 30);
            gridObj.GetComponent<GridLayoutGroup>().cellSize = new Vector2(150, 50);

            for (var x = 0; x < CurrentGame.Grid.Size; x++)
            for (var y = 0; y < CurrentGame.Grid.Size; y++) {
                //Clone and set up the button and hover triggers.
                var level = Object.Instantiate(AssetLoader.BingoCardButtonTemplate, gridObj.transform);

                //Label the button.
                var lvlCoords = $"{x}-{y}";
                if (level == null) continue;
                level.name = lvlCoords;
                var levelObject = CurrentGame.Grid.LevelTable[lvlCoords];
                var text = GetGameObjectChild(level, "Text");

                if (text != null) text.GetComponent<Text>().text = levelObject.LevelName;

                // Setup the BingoLevelData component.
                level.AddComponent<BingoLevelData>();
                level.GetComponent<BingoLevelData>().Column = x;
                level.GetComponent<BingoLevelData>().Row = y;
                level.GetComponent<BingoLevelData>().IsAngryLevel = levelObject.IsAngryLevel;
                level.GetComponent<BingoLevelData>().AngryParentBundle = levelObject.AngryParentBundle;
                level.GetComponent<BingoLevelData>().AngryLevelId = levelObject.AngryLevelId;
                level.GetComponent<BingoLevelData>().LevelName = levelObject.LevelName;

                // Set up the click listener.
                level.GetComponent<Button>().onClick.RemoveAllListeners();
                
                level.GetComponent<Button>().onClick.AddListener(delegate {
                    BingoMenuController.LoadBingoLevel(levelObject.LevelId, lvlCoords,
                        level.GetComponent<BingoLevelData>());
                });

                level.transform.SetParent(BingoCard.Grid?.transform);
                level.SetActive(true);
            }

            //Shift the position of the grid so it's better centered.
            switch (CurrentGame.Grid.Size) {
                case 3: {
                    gridObj.transform.localPosition = new Vector3(-200f, 100f, 0f);
                }
                    break;
                case 4: {
                    gridObj.transform.localPosition = new Vector3(-275f, 122f, 0f);
                }
                    break;
                case 5: {
                    gridObj.transform.localPosition = new Vector3(-350f, 145f, 0f);
                }
                    break;
            }
        }

        //Display teammates.
        var teammates = GetGameObjectChild(BingoCard.Teammates, "Players")?.GetComponent<TextMeshProUGUI>();

        if (teammates != null) {
            teammates.text = "";

            if (Teammates != null)
                foreach (var player in Teammates)
                    teammates.text += $"{player}\n";
        }

        //Reset votes.
        alreadyStartedVote = false;
    }

    public static async Task SetupGameDetails(Game game, string password, bool isHost = true) {
        CurrentGame = game;

        BingoEncapsulator.BingoMenu?.SetActive(false);
        BingoEncapsulator.BingoGameBrowser?.SetActive(false);

        //Small hack to fix the lobby UI elements & player list appearing invisible due to a base issue with TextMeshPro and how it works
        if (BingoEncapsulator.BingoLobbyScreen != null) {
            foreach (var text in BingoEncapsulator.BingoLobbyScreen.GetComponentsInChildren<TextMeshProUGUI>(true)) {
                await Task.Delay(1);
                text.ForceMeshUpdate();
            }

            BingoEncapsulator.BingoLobbyScreen.SetActive(true);
        }

        ShowGameId(password);
        RefreshPlayerList();

        if (BingoLobby.MaxPlayers is not null) BingoLobby.MaxPlayers.interactable = isHost;
        if (BingoLobby.MaxTeams is not null) BingoLobby.MaxTeams.interactable = isHost;
        if (BingoLobby.TimeLimit is not null) BingoLobby.TimeLimit.interactable = isHost;
        if (BingoLobby.TeamComposition is not null) BingoLobby.TeamComposition.interactable = isHost;
        if (BingoLobby.GridSize is not null) BingoLobby.GridSize.interactable = isHost;
        if (BingoLobby.Gamemode is not null) BingoLobby.Gamemode.interactable = isHost;
        if (BingoLobby.Difficulty is not null) BingoLobby.Difficulty.interactable = isHost;
        if (BingoLobby.RequirePRank is not null) BingoLobby.RequirePRank.interactable = isHost;
        if (BingoLobby.DisableCampaignAltExits is not null) BingoLobby.DisableCampaignAltExits.interactable = isHost;
        if (BingoLobby.GameVisibility is not null) BingoLobby.GameVisibility.interactable = isHost;
        BingoLobby.StartGame?.SetActive(isHost);
        BingoLobby.SelectMaps?.SetActive(isHost);
        BingoLobby.RoomIdDisplay?.SetActive(isHost);
        BingoLobby.CopyId?.SetActive(isHost);

        var btn = BingoLobby.SetTeams?.GetComponent<Button>();

        if (btn is not null) btn.interactable = isHost;

        if (isHost) {
            //Reset field settings to default values.
            if (BingoLobby.MaxPlayers is not null) BingoLobby.MaxPlayers.text = 8.ToString();
            if (BingoLobby.MaxTeams is not null) BingoLobby.MaxTeams.text = 4.ToString();
            if (BingoLobby.TeamComposition is not null) BingoLobby.TeamComposition.value = 0;
            if (BingoLobby.GridSize is not null) BingoLobby.GridSize.value = 0;
            if (BingoLobby.Gamemode is not null) BingoLobby.Gamemode.value = 0;
            if (BingoLobby.TimeLimit is not null) BingoLobby.TimeLimit.text = 5.ToString();
            if (BingoLobby.Difficulty is not null) BingoLobby.Difficulty.value = 2;
            if (BingoLobby.RequirePRank is not null) BingoLobby.RequirePRank.isOn = false;
            if (BingoLobby.DisableCampaignAltExits is not null) BingoLobby.DisableCampaignAltExits.isOn = false;
            if (BingoLobby.GameVisibility is not null) BingoLobby.GameVisibility.value = 0;

            BingoMapSelection.NumOfMapsTotal = 0;
            BingoMapSelection.UpdateNumber();
            BingoMapSelection.SelectedIds.Clear();

            if (BingoMapSelection.MapPoolButtons.Count > 0)
                foreach (var mapPoolButton in BingoMapSelection.MapPoolButtons) {
                    var img = GetGameObjectChild(mapPoolButton, "Image")?.GetComponent<Image>();

                    if (img is not null) img.color = new Color(1, 1, 1, 0);
                    mapPoolButton.GetComponent<MapPoolData>().mapPoolEnabled = false;
                }
        } else {
            if (BingoLobby.MaxPlayers is not null)
                BingoLobby.MaxPlayers.text = CurrentGame.GameSettings.MaxPlayers.ToString();
            if (BingoLobby.MaxTeams is not null)
                BingoLobby.MaxTeams.text = CurrentGame.GameSettings.MaxTeams.ToString();
            if (BingoLobby.TeamComposition is not null)
                BingoLobby.TeamComposition.value = CurrentGame.GameSettings.TeamComposition;
            if (BingoLobby.GridSize is not null) BingoLobby.GridSize.value = CurrentGame.GameSettings.GridSize;
            if (BingoLobby.Gamemode is not null) BingoLobby.Gamemode.value = CurrentGame.GameSettings.Gamemode;
            if (BingoLobby.TimeLimit is not null)
                BingoLobby.TimeLimit.text = CurrentGame.GameSettings.TimeLimit.ToString();
            if (BingoLobby.Difficulty is not null) BingoLobby.Difficulty.value = CurrentGame.GameSettings.Difficulty;
            if (BingoLobby.RequirePRank is not null)
                BingoLobby.RequirePRank.isOn = CurrentGame.GameSettings.RequiresPRank;
            if (BingoLobby.DisableCampaignAltExits is not null)
                BingoLobby.DisableCampaignAltExits.isOn = CurrentGame.GameSettings.DisableCampaignAltExits;
            if (BingoLobby.GameVisibility is not null)
                BingoLobby.GameVisibility.value = CurrentGame.GameSettings.GameVisibility;
        }

        NetworkManager.SetState(Types.State.InLobby);
        await NetworkManager.RegisterConnection();
    }

    private static void ShowGameId(string password) {
        var text = GetGameObjectChild(GetGameObjectChild(BingoLobby.RoomIdDisplay, "Title"), "Text")?
            .GetComponent<Text>();

        if (text != null) text.text = $"Game ID: {password}";
    }

    public static async Task StartGame() {
        await NetworkManager.SendStartGameSignal(CurrentGame.GameId);
    }

    public static void UpdateCards(int row, int column, string team, string playername, float newTime) {
        var coordLookup = $"{row}-{column}";

        if (!CurrentGame.Grid.LevelTable.ContainsKey(coordLookup)) {
            Logging.Error("RECEIVED AN INVALID GRID POSITION TO UPDATE!");
            Logging.Error(coordLookup);
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                "A level was claimed by someone but an <color=orange>invalid grid position</color> was given.\nCheck BepInEx console and report it to Clearwater!");
            return;
        }

        try {
            CurrentGame.Grid.LevelTable[coordLookup].ClaimedBy = team;
            CurrentGame.Grid.LevelTable[coordLookup].TimeToBeat = newTime;

            if (GetSceneName() == "Main Menu") {
                var bingoGrid = GetGameObjectChild(BingoEncapsulator.BingoCardScreen, "BingoGrid");
                var item = GetGameObjectChild(bingoGrid, coordLookup);

                if (item == null) return;

                item.GetComponent<Image>().color = BingoCardPauseMenu.TeamColors[team];
                item.GetComponent<BingoLevelData>().IsClaimed = true;
                item.GetComponent<BingoLevelData>().ClaimedTeam = team;
                item.GetComponent<BingoLevelData>().ClaimedPlayer = playername;
                item.GetComponent<BingoLevelData>().TimeRequirement = newTime;
            } else {
                if (!BingoCardPauseMenu.TeamColors.TryGetValue(team, out _)) {
                    Logging.Error("Unable to get color, throwing exception");
                } else {
                    var card = GetGameObjectChild(GetGameObjectChild(BingoCardPauseMenu.Root, "Card"), coordLookup);

                    //Pause menu card
                    if (card != null)
                        card
                            .GetComponent<Image>().color = BingoCardPauseMenu.TeamColors[team];

                    //In-game card
                    var coord = GetGameObjectChild(BingoCardPauseMenu.inGamePanel, coordLookup);

                    if (coord != null)
                        coord.GetComponent<Image>().color =
                            BingoCardPauseMenu.TeamColors[team];

                    var bld = card?.GetComponent<BingoLevelData>();

                    if (bld == null) return;

                    bld.IsClaimed = true;
                    bld.ClaimedTeam = team;
                    bld.ClaimedPlayer = playername;
                    bld.TimeRequirement = newTime;
                }
            }
        } catch (Exception e) {
            Logging.Error("THREW AN INVALID GRID POSITION TO UPDATE!");
            Logging.Error(e.ToString());
            Logging.Error(coordLookup);
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                "A level was claimed by someone but the grid could not be updated.\nCheck BepInEx console and report it to Clearwater!");
        }
    }

    public static async Task RequestReroll(int row, int column) {
        if (alreadyStartedVote) {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("You have already started a vote in this game.");
        } else {
            var rr = new RerollRequest {
                GameId = CurrentGame.GameId,
                SteamId = Steamworks.SteamClient.SteamId.ToString(),
                Row = row,
                Column = column,
                SteamTicket = NetworkManager.CreateRegisterTicket()
            };

            await NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(rr));
        }

        MonoSingleton<OptionsManager>.Instance.UnPause();
    }

    public static async Task PingMapForTeam(string team, int row, int column) {
        var mp = new MapPing {
            GameId = CurrentGame.GameId,
            Team = team,
            Row = row,
            Column = column,
            Ticket = NetworkManager.CreateRegisterTicket()
        };

        await NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(mp));
    }
}