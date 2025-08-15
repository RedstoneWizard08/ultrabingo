using System.Threading.Tasks;
using AngryLevelLoader.Fields;
using AngryLevelLoader.Managers;
using AngryLevelLoader.Notifications;
using TMPro;
using UltraBINGO.Components;
using UltraBINGO.Net;
using UltraBINGO.Types;
using UltraBINGO.Util;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI;

public static class BingoMenuController {
    public static string currentlyDownloadingLevel = "";


    public static bool CheckSteamAuthentication() {
        if (Main.UpdateAvailable) {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                "<color=orange>An update is available! Please update your mod to play Baphomet's Bingo.</color>");
            return false;
        }

        if (!Main.IsSteamAuthenticated) {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                "Unable to authenticate with Steam.\nYou must be connected to the Steam servers, and own a legal copy of ULTRAKILL to play Baphomet's Bingo.");
            return false;
        }

        return Main.IsSteamAuthenticated;
    }

    public static void LoadBingoLevel(string levelName, string levelCoords, BingoLevelData levelData) {
        //Make sure the game hasn't ended.
        if (GameManager.CurrentGame.IsGameFinished()) return;

        if (!GameManager.CurrentGame.IsGameFinished()) {
            //Force disable cheats and major assists, set difficulty to difficulty of the game set by the host.
            MonoSingleton<PrefsManager>.Instance.SetBool("majorAssist", false);
            MonoSingleton<AssistController>.Instance.cheatsEnabled = false;
            MonoSingleton<PrefsManager>.Instance.SetInt("difficulty", GameManager.CurrentGame.GameSettings.Difficulty);

            var row = int.Parse(levelCoords[0].ToString());
            var column = int.Parse(levelCoords[2].ToString());
            GameManager.IsInBingoLevel = true;

            //Check if the level we're going into is campaign or Angry.
            //If it's Angry, we need to do some checks if the level is downloaded before going in.
            if (levelData.IsAngryLevel) {
                HandleAngryLoad(levelData, row, column);
            } else {
                GameManager.UpdateGridPosition(row, column);
                SceneHelper.LoadScene(levelName);
                NetworkManager.SetState(Types.State.InGame);
            }
        }
    }

    public static ScriptManager.LoadScriptResult LoadAngryScript(string scriptName) {
        return ScriptManager.AttemptLoadScriptWithCertificate(scriptName);
    }

    public static async Task<bool> DownloadAngryScript(string scriptName) {
        var field = new ScriptUpdateNotification.ScriptUpdateProgressField {
            scriptName = scriptName,
            scriptStatus = ScriptUpdateNotification.ScriptUpdateProgressField.ScriptStatus.Download
        };
        field.StartDownload();
        while (field.downloading) await Task.Delay(500);
        if (field.isDone) {
            Logging.Warn("Download finished");
            return true;
        } else {
            Logging.Error("Download failed!");
            return false;
        }
    }

    public static async void HandleAngryLoad(BingoLevelData angryLevelData, int row = 0, int column = 0) {
        //Make sure the game hasn't ended.
        if (GameManager.CurrentGame.IsGameFinished()) return;

        //Prevent changing levels while downloading to avoid problems.
        if (GameManager.IsDownloadingLevel == true) {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                "Please wait for the current download to complete before switching to a different level.");
            return;
        }

        //First check if the level exists locally, and is up to date.
        var isLevelReady = OnlineLevelsManager.onlineLevels[angryLevelData.AngryParentBundle]._status ==
                           OnlineLevelField.OnlineLevelStatus.installed;

        //If already downloaded and up to date, load the bundle.
        if (isLevelReady) {
            var locallyDownloadedLevels = AngryLevelLoader.Plugin.angryBundles;
            var bundleContainer = locallyDownloadedLevels[angryLevelData.AngryParentBundle];

            Logging.Message("Loading Angry level");
            //Need to (re)load the bundle before accessing it to make sure the level fields are accessible.
            GameManager.EnteringAngryLevel = true;
            await bundleContainer.UpdateScenes(true, false);
            await Task.Delay(250);

            //Make sure the given angry level ID exists inside the bundle...
            var levelsInBundle = bundleContainer.levels;
            var containsLevel = levelsInBundle.TryGetValue(angryLevelData.AngryLevelId, out var customLevel);
            if (containsLevel) {
                //...if it does, gather all the necessary data and ask Angry to load it.
                var msg = GetSceneName() != "Main Menu"
                    ? $"MOVING TO <color=orange>{angryLevelData.LevelName}</color>..."
                    : $"LOADING <color=orange>{angryLevelData.LevelName}</color>...";

                if (GetSceneName() == "Main Menu")
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
                else
                    BingoCardPauseMenu.DescriptorText.GetComponent<TextMeshProUGUI>().text = msg;
                await Task.Delay(1000);
                NetworkManager.SetState(Types.State.InGame);
                AngryLevelLoader.Plugin.selectedDifficulty = GameManager.CurrentGame.GameSettings.Difficulty;


                //Before loading, check if the level uses any custom scripts.
                var requiredAngryScripts = ScriptManager.GetRequiredScriptsFromBundle(bundleContainer);
                if (requiredAngryScripts.Count > 0) {
                    Logging.Message("Level requires custom scripts, checking if locally loaded");

                    //If they do, check if the scripts are already downloaded and loaded locally.
                    foreach (var scriptName in requiredAngryScripts)
                        if (!ScriptManager.ScriptExists(scriptName)) {
                            Logging.Message($"Asking Angry to download {scriptName}");
                            var downloadResult = await DownloadAngryScript(scriptName);
                            if (downloadResult == true) {
                                var res = LoadAngryScript(scriptName);
                                if (res != ScriptManager.LoadScriptResult.Loaded) {
                                    Logging.Error("Failed to load script with reason: ");
                                    Logging.Error(res.ToString());
                                }
                            }
                        } else {
                            Logging.Message($"{scriptName} is already downloaded");
                        }

                    if (!GameManager.CurrentGame.IsGameFinished()) {
                        GameManager.IsSwitchingLevels = true;
                        AngryLevelLoader.Plugin.difficultyField.gamemodeListValueIndex = 0; //Prevent nomo override

                        GameManager.UpdateGridPosition(row, column);
                        AngrySceneManager.LoadLevelWithScripts(requiredAngryScripts, bundleContainer, customLevel,
                            customLevel.data, customLevel.data.scenePath);
                    }
                } else {
                    if (!GameManager.CurrentGame.IsGameFinished()) {
                        GameManager.IsSwitchingLevels = true;
                        AngryLevelLoader.Plugin.difficultyField.gamemodeListValueIndex = 0; //Prevent nomo override
                        GameManager.UpdateGridPosition(row, column);
                        AngrySceneManager.LoadLevel(bundleContainer, customLevel, customLevel.data,
                            customLevel.data.scenePath, true);
                    }
                }
            } else {
                Logging.Error("Given level ID does not exist inside the bundle!");
                Logging.Error($"Given level ID: {angryLevelData.AngryLevelId}");
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                    "<color=orange>Failed to load level, something went wrong.</color>");
            }
        } else {
            //Prevent multiple downloads.
            if (GameManager.IsDownloadingLevel) {
                Logging.Warn("Trying to download a level but another one is already in progress!");
                return;
            }

            //If level does not already exist locally, get Angry to download it first.
            Logging.Message("Level does not already exist locally - Downloading from online repo");

            if (GetSceneName() != "Main Menu") MonoSingleton<OptionsManager>.Instance.UnPause();
            GameManager.IsDownloadingLevel = true;

            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                $"-- DOWNLOADING {angryLevelData.LevelName} --\nYou can continue to play in the meantime.");
            currentlyDownloadingLevel = angryLevelData.LevelName;
            OnlineLevelsManager.onlineLevels[angryLevelData.AngryParentBundle].Download();
            while (OnlineLevelsManager.onlineLevels[angryLevelData.AngryParentBundle].downloading)
                await Task.Delay(500);
        }
    }

    public static async void LoadBingoLevelFromPauseMenu(string levelCoords, BingoLevelData levelData) {
        //Make sure the game hasn't ended or we're not already loading a level.
        if (GameManager.CurrentGame.IsGameFinished() || GameManager.IsSwitchingLevels) return;

        //Prevent loading the level we're already on.
        if (levelData.AngryLevelId == GetSceneName()) {
            Logging.Warn("Trying to load level we're already in");
            return;
        }

        if (!GameManager.CurrentGame.IsGameFinished()) {
            var row = int.Parse(levelCoords[0].ToString());
            var column = int.Parse(levelCoords[2].ToString());

            var levelDisplayName = GameManager.CurrentGame.Grid.LevelTable[levelCoords].LevelName;
            var levelId = GameManager.CurrentGame.Grid.LevelTable[levelCoords].LevelId;

            //Save vote data if a vote is ongoing, so the panel can reappear after scene switch.
            GameManager.VoteData = MonoSingleton<BingoVoteManager>.Instance.voteOngoing
                ? new VoteData(true, MonoSingleton<BingoVoteManager>.Instance.hasVoted,
                    MonoSingleton<BingoVoteManager>.Instance.voteThreshold,
                    MonoSingleton<BingoVoteManager>.Instance.currentVotes, MonoSingleton<BingoVoteManager>.Instance.map,
                    MonoSingleton<BingoVoteManager>.Instance.timeRemaining)
                : new VoteData(false);

            if (levelData.IsAngryLevel) {
                HandleAngryLoad(levelData, row, column);
            } else {
                var msg = $"MOVING TO <color=orange>{levelDisplayName}</color>...";
                BingoCardPauseMenu.DescriptorText.GetComponent<TextMeshProUGUI>().text = msg;
                GameManager.IsSwitchingLevels = true;

                await Task.Delay(1000);
                //Check if game hasn't ended between click and delay. If it has, prevent level load.
                if (!GameManager.CurrentGame.IsGameFinished()) {
                    NetworkManager.SetState(Types.State.InGame);
                    GameManager.UpdateGridPosition(row, column);
                    SceneHelper.LoadScene(levelId);
                }
            }
        }
    }

    public static void ReturnToMenu() {
        UIManager.RemoveLimit();
        BingoEncapsulator.Root.SetActive(false);
        GetGameObjectChild(GetInactiveRootObject("Canvas"), "Difficulty Select (1)").SetActive(true);
        NetworkManager.SetState(Types.State.Normal);
    }

    public static void CreateRoom() {
        if (!CheckSteamAuthentication()) return;

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Creating room...");
        NetworkManager.pendingAction = AsyncAction.Host;
        NetworkManager.ConnectWebSocket();
    }

    public static void JoinRoom(string roomPassword) {
        if (!CheckSteamAuthentication()) return;
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Joining room...");
        NetworkManager.pendingAction = AsyncAction.Join;
        NetworkManager.pendingPassword = roomPassword;
        NetworkManager.ConnectWebSocket();
    }

    public static void StartGame(int gameType) {
        GameManager.SetupBingoCardDynamic();

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("The game has begun!");

        if (GameManager.CurrentGame.GameSettings.Gamemode == 1) {
            var canvas = GetInactiveRootObject("Canvas");
            canvas.AddComponent<DominationTimeManager>();
        }

        GameManager.MoveToCard(gameType);
    }
}