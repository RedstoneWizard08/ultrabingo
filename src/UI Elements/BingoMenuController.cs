using System.Collections.Generic;
using System.Threading.Tasks;
using AngryLevelLoader.Containers;
using AngryLevelLoader.Managers;
using UltraBINGO.Components;
using UltrakillBingoClient;
using UnityEngine;
using static UltraBINGO.CommonFunctions;


namespace UltraBINGO.UI_Elements;

public static class BingoMenuController
{
    public static bool checkSteamAuthentication()
    {
        if(!Main.isSteamAuthenticated)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Unable to authenticate with Steam.\nYou must be connected to the Steam servers, and own a legal copy of ULTRAKILL to play Baphomet's Bingo.");
        }
        return Main.isSteamAuthenticated;
    }
    
    public static async void LoadBingoLevel(string levelName, string levelCoords, BingoLevelData levelData)
    {
        if(!GameManager.CurrentGame.isGameFinished())
        {
            //Force disable cheats and major assists, set difficulty to difficulty of the game set by the host.
            MonoSingleton<PrefsManager>.Instance.SetBool("majorAssist", false);
            MonoSingleton<AssistController>.Instance.cheatsEnabled = false;
            MonoSingleton<PrefsManager>.Instance.SetInt("difficulty", GameManager.CurrentGame.gameSettings.difficulty);
        
            int row = int.Parse(levelCoords[0].ToString());
            int column = int.Parse(levelCoords[2].ToString());
            GameManager.isInBingoLevel = true;
            GameManager.currentRow = row;
            GameManager.currentColumn = column;
        
        
            //Check if the level we're going into is campaign or Angry.
            //If it's Angry, we need to do some checks if the level is downloaded before going in.
            if(levelData.isAngryLevel)
            {
                handleAngryLoad(levelData);
            }
            else
            {
                SceneHelper.LoadScene(levelName);
            }
        }

    }
    
    public static async void handleAngryLoad(BingoLevelData angryLevelData,bool isInGame=false)
    {
            //Prevent changing levels while downloading to avoid problems.
            if(GameManager.isDownloadingLevel == true)
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Please wait for the current download to complete before switching to a different level.");
                return;
            }
        
            //First check if the level exists locally.
            Logging.Message("Checking if level exists locally");
            Dictionary<string,AngryBundleContainer> locallyDownloadedLevels = AngryLevelLoader.Plugin.angryBundles;
            bool isAlreadyDownloaded = locallyDownloadedLevels.TryGetValue(angryLevelData.angryParentBundle, out AngryBundleContainer bundleContainer);
            
            //If already downloaded and up to date, load the bundle.
            if(isAlreadyDownloaded)
            {
                //Need to (re)load the bundle before accessing it to make sure the level fields are accessible.
                Logging.Message("Level bundle exists locally, loading bundle");
                GameManager.enteringAngryLevel = true;
                await bundleContainer.UpdateScenes(true,false);
                await Task.Delay(250);
                
                //Make sure the given angry level ID exists inside the bundle...
                Dictionary<string,LevelContainer> levelsInBundle = bundleContainer.levels;
                bool containsLevel = levelsInBundle.TryGetValue(angryLevelData.angryLevelId, out LevelContainer customLevel);
                if(containsLevel)
                {
                    //...if it does, gather all the necessary data and ask Angry to load it.
                    Logging.Message("Loading specified Angry level");
                    string msg = (getSceneName() != "Main Menu" ? "Moving to " + angryLevelData.levelName + "..." : "Loading " + angryLevelData.levelName + "...");
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);

                    AngryLevelLoader.Plugin.selectedDifficulty = GameManager.CurrentGame.gameSettings.difficulty;
                    AngrySceneManager.LoadLevel(bundleContainer,customLevel,customLevel.data,customLevel.data.scenePath,true);
                }
                else
                {
                    Logging.Error("Given level ID does not exist inside the bundle!");
                    Logging.Error("Given level ID: " + angryLevelData.angryLevelId);
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=orange>Failed to load level, something went wrong.</color>");
                }
            }
            else
            {
                //Prevent multiple downloads.
                if(GameManager.isDownloadingLevel == true)
                {
                    Logging.Warn("Trying to download a level but another one is already in progress!");
                    return;
                }
                
                //If level does not already exist locally, get Angry to download it first.
                Logging.Warn("Level does not already exist locally - Downloading from online repo");
                GameManager.isDownloadingLevel = true;
                
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("-- DOWNLOADING LEVEL BUNDLE - <color=orange>PLEASE WAIT A MOMENT...</color> --");
                OnlineLevelsManager.onlineLevels[angryLevelData.angryParentBundle].Download();
                while(OnlineLevelsManager.onlineLevels[angryLevelData.angryParentBundle].downloading)
                {
                    Logging.Message("Waiting");
                    await Task.Delay(500);
                }
            }
    }
    
    public static async void LoadBingoLevelFromPauseMenu(string levelCoords, BingoLevelData levelData)
    {
        if(!GameManager.CurrentGame.isGameFinished())
        {
            int row = int.Parse(levelCoords[0].ToString());
            int column = int.Parse(levelCoords[2].ToString());
        
            GameManager.currentRow = row;
            GameManager.currentColumn = column;
        
            string levelDisplayName = GameManager.CurrentGame.grid.levelTable[levelCoords].levelName;
            string levelId = GameManager.CurrentGame.grid.levelTable[levelCoords].levelId;
        
            if(levelData.isAngryLevel)
            {
                handleAngryLoad(levelData);
            }
            else
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Moving to "+levelDisplayName + "...");
                await Task.Delay(1000);
                SceneHelper.LoadScene(levelId);
            }
        }
    }
    
    public static void ReturnToMenu(GameObject bingoMenu)
    {
        Logging.Message("Exiting menu");
        BingoEncapsulator.Root.SetActive(false);
        GetGameObjectChild(bingoMenu.transform.parent.parent.gameObject,"Difficulty Select (1)").SetActive(true);
    }
    
    public static void CreateRoom()
    {
        if(!checkSteamAuthentication())
        {
            return;
        }
        
        //Check if the websocket connection is up.
        if(!NetworkManager.isConnectionUp())
        {
            NetworkManager.ConnectWebSocket();
        }
        
        if(!NetworkManager.isConnectionUp())
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=red>Failed to connect to server.</color>");
            return;
        }
        
        NetworkManager.CreateRoom();
    }
    
    public static void JoinRoom(int roomId)
    {
        if(!checkSteamAuthentication())
        {
            return;
        }
        
        if(!NetworkManager.isConnectionUp())
        {
            NetworkManager.ConnectWebSocket();
        }
        if(!NetworkManager.isConnectionUp())
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=red>Failed to connect to server.</color>");
            return;
        }
        NetworkManager.JoinGame(roomId);
    }
    
    public static void StartGame()
    {
        GameManager.SetupBingoCardDynamic();
        
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("The game has begun!");
        GameManager.MoveToCard();
    }
    
}