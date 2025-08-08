using TMPro;
using UltraBINGO.NetworkMessages;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoLobby {
    public static GameObject? PlayerList;
    private static GameObject? _returnToBingoMenu;
    public static GameObject? SelectMaps;
    public static GameObject? SetTeams;
    public static GameObject? StartGame;
    public static GameObject? RoomIdDisplay;
    public static GameObject? CopyId;

    private static GameObject? _gameOptions;
    public static TMP_InputField? MaxPlayers;
    public static TMP_InputField? MaxTeams;
    public static TMP_InputField? TimeLimit;
    public static TMP_Dropdown? Gamemode;
    public static TMP_Dropdown? TeamComposition;
    public static TMP_Dropdown? GridSize;
    public static TMP_Dropdown? Difficulty;
    public static Toggle? RequirePRank;
    public static Toggle? DisableCampaignAltExits;
    public static TMP_Dropdown? GameVisibility;

    private static void OnMaxPlayerUpdate(string playerAmount) {
        var amount = int.Parse(playerAmount);
        GameManager.CurrentGame.gameSettings.maxPlayers =
            Mathf.Clamp(Mathf.Max(amount, GameManager.CurrentGame.currentPlayers.Count), 2, 16);

        if (MaxPlayers != null)
            MaxPlayers.text = Mathf.Clamp(amount, Mathf.Max(amount, GameManager.CurrentGame.currentPlayers.Count), 16f)
                .ToString();

        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnMaxTeamUpdate(string teamAmount) {
        var amount = int.Parse(teamAmount);
        GameManager.CurrentGame.gameSettings.maxTeams = Mathf.Clamp(amount, 2, 4);
        if (MaxTeams != null) MaxTeams.text = Mathf.Clamp(amount, 2f, 4f).ToString();
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnTimeLimitUpdate(string timeLimit) {
        var amount = int.Parse(timeLimit);
        GameManager.CurrentGame.gameSettings.timeLimit = Mathf.Clamp(amount, 5, 30);
        if (TimeLimit != null) TimeLimit.text = Mathf.Clamp(amount, 5, 30).ToString();
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnGamemodeTypeUpdate(int value) {
        GameManager.CurrentGame.gameSettings.gamemode = value;
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnTeamCompositionUpdate(int value) {
        GameManager.CurrentGame.gameSettings.teamComposition = value;
        SetTeams?.SetActive(value == 1 && GameManager.PlayerIsHost());
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnGridSizeUpdate(int value) {
        if (GridSize != null) GridSize.value = value;
        GameManager.CurrentGame.gameSettings.gridSize = value;
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnDifficultyUpdate(int value) {
        GameManager.CurrentGame.gameSettings.difficulty = value;
        if (Difficulty != null) Difficulty.value = value;
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnPRankRequiredUpdate(bool value) {
        if (RequirePRank != null) RequirePRank.isOn = value;
        GameManager.CurrentGame.gameSettings.requiresPRank = value;
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnDisableCampaignAltExitsUpdate(bool value) {
        if (DisableCampaignAltExits != null) DisableCampaignAltExits.isOn = value;
        GameManager.CurrentGame.gameSettings.disableCampaignAltExits = value;
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnGameVisibilityUpdate(int value) {
        if (GameVisibility != null) GameVisibility.value = value;
        GameManager.CurrentGame.gameSettings.gameVisibility = value;
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    public static void UpdateFromNotification(UpdateRoomSettingsNotification newSettings) {
        if (MaxPlayers is not null) MaxPlayers.text = newSettings.maxPlayers.ToString();
        if (MaxTeams is not null) MaxTeams.text = newSettings.maxTeams.ToString();
        if (TimeLimit is not null) TimeLimit.text = newSettings.timeLimit.ToString();
        if (Gamemode is not null) Gamemode.value = newSettings.gamemode;
        if (TeamComposition is not null) TeamComposition.value = newSettings.teamComposition;
        if (RequirePRank is not null) RequirePRank.isOn = newSettings.PRankRequired;
        if (Difficulty is not null) Difficulty.value = newSettings.difficulty;
        if (GridSize is not null) GridSize.value = newSettings.gridSize;
        if (DisableCampaignAltExits is not null) DisableCampaignAltExits.isOn = newSettings.disableCampaignAltExits;
        if (GameVisibility is not null) GameVisibility.value = newSettings.gameVisibility;

        GameManager.CurrentGame.gameSettings.maxPlayers = newSettings.maxPlayers;
        GameManager.CurrentGame.gameSettings.maxTeams = newSettings.maxTeams;
        GameManager.CurrentGame.gameSettings.timeLimit = newSettings.timeLimit;
        GameManager.CurrentGame.gameSettings.gamemode = newSettings.gamemode;
        GameManager.CurrentGame.gameSettings.teamComposition = newSettings.teamComposition;
        GameManager.CurrentGame.gameSettings.requiresPRank = newSettings.PRankRequired;
        GameManager.CurrentGame.gameSettings.difficulty = newSettings.difficulty;
        GameManager.CurrentGame.gameSettings.gridSize = newSettings.gridSize;
        GameManager.CurrentGame.gameSettings.disableCampaignAltExits = newSettings.disableCampaignAltExits;
        GameManager.CurrentGame.gameSettings.gameVisibility = newSettings.gameVisibility;
    }

    private static void LockUI() {
        if (StartGame is not null) StartGame.GetComponent<Button>().interactable = false;
        if (_returnToBingoMenu is not null) _returnToBingoMenu.GetComponent<Button>().interactable = false;
        if (SelectMaps is not null) SelectMaps.GetComponent<Button>().interactable = false;
    }

    public static void UnlockUI() {
        if (StartGame is not null) StartGame.GetComponent<Button>().interactable = true;
        if (_returnToBingoMenu is not null) _returnToBingoMenu.GetComponent<Button>().interactable = true;
        if (SelectMaps is not null) SelectMaps.GetComponent<Button>().interactable = true;
    }

    public static void Init(ref GameObject bingoLobby) {
        //Player list
        PlayerList = GetGameObjectChild(bingoLobby, "BingoLobbyPlayers");

        //Leave game button
        _returnToBingoMenu = GetGameObjectChild(bingoLobby, "LeaveGame");

        _returnToBingoMenu?.GetComponent<Button>().onClick.AddListener(delegate { GameManager.LeaveGame().Wait(); });

        SelectMaps = GetGameObjectChild(bingoLobby, "SelectMaps");

        SelectMaps?.GetComponent<Button>().onClick.AddListener(delegate {
            BingoEncapsulator.BingoLobbyScreen.SetActive(false);
            BingoEncapsulator.BingoMapSelectionMenu.SetActive(true);
            BingoMapSelection.Setup();
        });

        SetTeams = GetGameObjectChild(bingoLobby, "SetTeams");

        SetTeams?.GetComponent<Button>().onClick.AddListener(delegate {
            BingoSetTeamsMenu.Setup();
            BingoEncapsulator.BingoLobbyScreen.SetActive(false);
            BingoEncapsulator.BingoSetTeams.SetActive(true);
        });

        //Start game button
        StartGame = GetGameObjectChild(bingoLobby, "StartGame");

        StartGame?.GetComponent<Button>().onClick.AddListener(delegate {
            if (!GameManager.PreStartChecks()) return;

            //Lock the button to prevent being able to spam it
            LockUI();
            GameManager.StartGame();
        });

        //Room id text
        RoomIdDisplay = GetGameObjectChild(bingoLobby, "BingoGameID");

        //Copy ID
        CopyId = GetGameObjectChild(bingoLobby, "CopyID");

        CopyId?.GetComponent<Button>().onClick.AddListener(delegate {
            GUIUtility.systemCopyBuffer = GetGameObjectChild(GetGameObjectChild(RoomIdDisplay, "Title"), "Text")?
                .GetComponent<Text>().text.Split(':')[1];
        });

        //Game options
        _gameOptions = GetGameObjectChild(bingoLobby, "BingoGameSettings");

        MaxPlayers = GetGameObjectChild(GetGameObjectChild(_gameOptions, "MaxPlayers"), "Input")?
            .GetComponent<TMP_InputField>();
        MaxPlayers?.onEndEdit.AddListener(OnMaxPlayerUpdate);

        MaxTeams = GetGameObjectChild(GetGameObjectChild(_gameOptions, "MaxTeams"), "Input")?
            .GetComponent<TMP_InputField>();
        MaxTeams?.onEndEdit.AddListener(OnMaxTeamUpdate);

        TimeLimit = GetGameObjectChild(GetGameObjectChild(_gameOptions, "TimeLimit"), "Input")?
            .GetComponent<TMP_InputField>();
        TimeLimit?.onEndEdit.AddListener(OnTimeLimitUpdate);

        TeamComposition = GetGameObjectChild(GetGameObjectChild(_gameOptions, "TeamComposition"), "Dropdown")?
            .GetComponent<TMP_Dropdown>();
        TeamComposition?.onValueChanged.AddListener(OnTeamCompositionUpdate);

        Gamemode = GetGameObjectChild(GetGameObjectChild(_gameOptions, "Gamemode"), "Dropdown")?
            .GetComponent<TMP_Dropdown>();
        Gamemode?.onValueChanged.AddListener(OnGamemodeTypeUpdate);

        GridSize = GetGameObjectChild(GetGameObjectChild(_gameOptions, "GridSize"), "Dropdown")?
            .GetComponent<TMP_Dropdown>();
        GridSize?.onValueChanged.AddListener(OnGridSizeUpdate);

        Difficulty = GetGameObjectChild(GetGameObjectChild(_gameOptions, "Difficulty"), "Dropdown")?
            .GetComponent<TMP_Dropdown>();
        Difficulty?.onValueChanged.AddListener(OnDifficultyUpdate);

        RequirePRank = GetGameObjectChild(GetGameObjectChild(_gameOptions, "RequirePRank"), "Input")?
            .GetComponent<Toggle>();
        RequirePRank?.onValueChanged.AddListener(OnPRankRequiredUpdate);

        DisableCampaignAltExits =
            GetGameObjectChild(GetGameObjectChild(_gameOptions, "DisableCampaignAltEnds"), "Input")?
                .GetComponent<Toggle>();
        DisableCampaignAltExits?.onValueChanged.AddListener(OnDisableCampaignAltExitsUpdate);

        GameVisibility = GetGameObjectChild(GetGameObjectChild(_gameOptions, "GameVisibility"), "Dropdown")?
            .GetComponent<TMP_Dropdown>();
        GameVisibility?.onValueChanged.AddListener(OnGameVisibilityUpdate);

        bingoLobby.SetActive(false);
    }
}