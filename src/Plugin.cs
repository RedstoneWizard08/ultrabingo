
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using TMPro;
using UltraBINGO;
using UltraBINGO.UI_Elements;
using UnityEngine;
using UnityEngine.SceneManagement;
using LogType = UnityEngine.LogType;

using WebSocketSharp;

using static UltraBINGO.CommonFunctions;

namespace UltrakillBingoClient
{
    [BepInPlugin(Main.pluginId, Main.pluginName, Main.pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginId = "clearwater.ultrakillbingo.ultrakillbingo";
        public const string pluginName = "UltraBINGO";
        public const string pluginVersion = "0.0.1";
        
        public static bool IsDevelopmentBuild = false;
        
        public static string ModFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        /*
         * UltraBINGO
         *
         * Adds a bingo multiplayer gamemode.
         * 
         * Created by Clearwater.
         * */
        
        
        private void Awake()
        {
            
            // Plugin startup logic
            Debug.unityLogger.filterLogType = LogType.Exception;
            
            Logging.Message("--Now loading UltraBingo...--");
            if(Main.IsDevelopmentBuild)
            {
                Logging.Warn("-- DEVELOPMENT BUILD. REQUESTS WILL BE SENT TO LOCALHOST. --");
            }
            else
            {
                Logging.Warn("-- RELEASE BUILD. REQUESTS WILL BE SENT TO REMOTE SERVER. --");
            }
            
            Logging.Message("--Loading from assetbundle...--");
            AssetLoader.LoadAssets();
            
            Harmony harmony = new Harmony(pluginId);
            harmony.PatchAll();
            
            
            SceneManager.sceneLoaded += onSceneLoaded;
            
            NetworkManager.initialise();
        }
        
        
        public void onSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameManager.hasSent = false;
            if(getSceneName() == "Main Menu")
            {
                if(GameManager.CurrentGame != null && GameManager.CurrentGame.isGameFinished())
                {
                    Logging.Message("Game is over, showing end screen");
                    BingoEnd.ShowEndScreen();
                }
            }
        }
    }
}