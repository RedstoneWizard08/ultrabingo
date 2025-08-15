using TMPro;
using UltraBINGO.Packets;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.UI;

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
        GameManager.CurrentGame.GameSettings.MaxPlayers =
            Mathf.Clamp(Mathf.Max(amount, GameManager.CurrentGame.CurrentPlayers.Count), 2, 16);

        if (MaxPlayers != null)
            MaxPlayers.text = Mathf.Clamp(amount, Mathf.Max(amount, GameManager.CurrentGame.CurrentPlayers.Count), 16f)
                .ToString();

        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnMaxTeamUpdate(string teamAmount) {
        var amount = int.Parse(teamAmount);
        GameManager.CurrentGame.GameSettings.MaxTeams = Mathf.Clamp(amount, 2, 4);
        if (MaxTeams != null) MaxTeams.text = Mathf.Clamp(amount, 2f, 4f).ToString();
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnTimeLimitUpdate(string timeLimit) {
        var amount = int.Parse(timeLimit);
        GameManager.CurrentGame.GameSettings.TimeLimit = Mathf.Clamp(amount, 5, 30);
        if (TimeLimit != null) TimeLimit.text = Mathf.Clamp(amount, 5, 30).ToString();
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnGamemodeTypeUpdate(int value) {
        GameManager.CurrentGame.GameSettings.Gamemode = value;
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnTeamCompositionUpdate(int value) {
        GameManager.CurrentGame.GameSettings.TeamComposition = value;
        SetTeams?.SetActive(value == 1 && GameManager.PlayerIsHost());
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnGridSizeUpdate(int value) {
        if (GridSize != null) GridSize.value = value;
        GameManager.CurrentGame.GameSettings.GridSize = value;
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnDifficultyUpdate(int value) {
        GameManager.CurrentGame.GameSettings.Difficulty = value;
        if (Difficulty != null) Difficulty.value = value;
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnPRankRequiredUpdate(bool value) {
        if (RequirePRank != null) RequirePRank.isOn = value;
        GameManager.CurrentGame.GameSettings.RequiresPRank = value;
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnDisableCampaignAltExitsUpdate(bool value) {
        if (DisableCampaignAltExits != null) DisableCampaignAltExits.isOn = value;
        GameManager.CurrentGame.GameSettings.DisableCampaignAltExits = value;
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    private static void OnGameVisibilityUpdate(int value) {
        if (GameVisibility != null) GameVisibility.value = value;
        GameManager.CurrentGame.GameSettings.GameVisibility = value;
        UIManager.HandleGameSettingsUpdate().Wait();
    }

    public static void UpdateFromNotification(UpdateRoomSettingsNotification newSettings) {
        if (MaxPlayers is not null) MaxPlayers.text = newSettings.MaxPlayers.ToString();
        if (MaxTeams is not null) MaxTeams.text = newSettings.MaxTeams.ToString();
        if (TimeLimit is not null) TimeLimit.text = newSettings.TimeLimit.ToString();
        if (Gamemode is not null) Gamemode.value = newSettings.GameMode;
        if (TeamComposition is not null) TeamComposition.value = newSettings.TeamComposition;
        if (RequirePRank is not null) RequirePRank.isOn = newSettings.PRankRequired;
        if (Difficulty is not null) Difficulty.value = newSettings.Difficulty;
        if (GridSize is not null) GridSize.value = newSettings.GridSize;
        if (DisableCampaignAltExits is not null) DisableCampaignAltExits.isOn = newSettings.DisableCampaignAltExits;
        if (GameVisibility is not null) GameVisibility.value = newSettings.GameVisibility;

        GameManager.CurrentGame.GameSettings.MaxPlayers = newSettings.MaxPlayers;
        GameManager.CurrentGame.GameSettings.MaxTeams = newSettings.MaxTeams;
        GameManager.CurrentGame.GameSettings.TimeLimit = newSettings.TimeLimit;
        GameManager.CurrentGame.GameSettings.Gamemode = newSettings.GameMode;
        GameManager.CurrentGame.GameSettings.TeamComposition = newSettings.TeamComposition;
        GameManager.CurrentGame.GameSettings.RequiresPRank = newSettings.PRankRequired;
        GameManager.CurrentGame.GameSettings.Difficulty = newSettings.Difficulty;
        GameManager.CurrentGame.GameSettings.GridSize = newSettings.GridSize;
        GameManager.CurrentGame.GameSettings.DisableCampaignAltExits = newSettings.DisableCampaignAltExits;
        GameManager.CurrentGame.GameSettings.GameVisibility = newSettings.GameVisibility;
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
        PlayerList = FindObject(bingoLobby, "BingoLobbyPlayers");

        //Leave game button
        _returnToBingoMenu = FindObject(bingoLobby, "LeaveGame");

        _returnToBingoMenu?.GetComponent<Button>().onClick.AddListener(delegate { GameManager.LeaveGame().Wait(); });

        SelectMaps = FindObject(bingoLobby, "SelectMaps");

        SelectMaps?.GetComponent<Button>().onClick.AddListener(
            delegate {
                BingoEncapsulator.BingoLobbyScreen?.SetActive(false);
                BingoEncapsulator.BingoMapSelectionMenu?.SetActive(true);
                BingoMapSelection.Setup().Wait();
            }
        );

        SetTeams = FindObject(bingoLobby, "SetTeams");

        SetTeams?.GetComponent<Button>().onClick.AddListener(
            delegate {
                BingoSetTeamsMenu.Setup();
                BingoEncapsulator.BingoLobbyScreen?.SetActive(false);
                BingoEncapsulator.BingoSetTeams?.SetActive(true);
            }
        );

        //Start game button
        StartGame = FindObject(bingoLobby, "StartGame");

        StartGame?.GetComponent<Button>().onClick.AddListener(
            delegate {
                if (!GameManager.PreStartChecks()) return;

                //Lock the button to prevent being able to spam it
                LockUI();
                GameManager.StartGame().Wait();
            }
        );

        //Room id text
        RoomIdDisplay = FindObject(bingoLobby, "BingoGameID");

        //Copy ID
        CopyId = FindObject(bingoLobby, "CopyID");

        CopyId?.GetComponent<Button>().onClick.AddListener(
            delegate {
                GUIUtility.systemCopyBuffer = FindObject(RoomIdDisplay, "Title", "Text")?
                    .GetComponent<Text>().text.Split(':')[1];
            }
        );

        //Game options
        _gameOptions = FindObject(bingoLobby, "BingoGameSettings");

        MaxPlayers = FindObject(_gameOptions, "MaxPlayers", "Input")?.GetComponent<TMP_InputField>();
        MaxPlayers?.onEndEdit.AddListener(OnMaxPlayerUpdate);

        MaxTeams = FindObject(_gameOptions, "MaxTeams", "Input")?.GetComponent<TMP_InputField>();
        MaxTeams?.onEndEdit.AddListener(OnMaxTeamUpdate);

        TimeLimit = FindObject(_gameOptions, "TimeLimit", "Input")?.GetComponent<TMP_InputField>();
        TimeLimit?.onEndEdit.AddListener(OnTimeLimitUpdate);

        TeamComposition = FindObject(_gameOptions, "TeamComposition", "Dropdown")?.GetComponent<TMP_Dropdown>();
        TeamComposition?.onValueChanged.AddListener(OnTeamCompositionUpdate);

        Gamemode = FindObject(_gameOptions, "Gamemode", "Dropdown")?.GetComponent<TMP_Dropdown>();
        Gamemode?.onValueChanged.AddListener(OnGamemodeTypeUpdate);

        GridSize = FindObject(_gameOptions, "GridSize", "Dropdown")?.GetComponent<TMP_Dropdown>();
        GridSize?.onValueChanged.AddListener(OnGridSizeUpdate);

        Difficulty = FindObject(_gameOptions, "Difficulty", "Dropdown")?.GetComponent<TMP_Dropdown>();
        Difficulty?.onValueChanged.AddListener(OnDifficultyUpdate);

        RequirePRank = FindObject(_gameOptions, "RequirePRank", "Input")?.GetComponent<Toggle>();
        RequirePRank?.onValueChanged.AddListener(OnPRankRequiredUpdate);

        DisableCampaignAltExits = FindObject(_gameOptions, "DisableCampaignAltEnds", "Input")?.GetComponent<Toggle>();
        DisableCampaignAltExits?.onValueChanged.AddListener(OnDisableCampaignAltExitsUpdate);

        GameVisibility = FindObject(_gameOptions, "GameVisibility", "Dropdown")?.GetComponent<TMP_Dropdown>();
        GameVisibility?.onValueChanged.AddListener(OnGameVisibilityUpdate);

        bingoLobby.SetActive(false);
    }
}