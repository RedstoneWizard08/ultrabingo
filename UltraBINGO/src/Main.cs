using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UltraBINGO.Net;
using UltraBINGO.Packets;
using UltraBINGO.UI;
using UltraBINGO.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UltraBINGO.CommonFunctions;

/*
 * Baphomet's Bingo
 *
 * Adds a bingo multiplayer gamemode.
 *
 * Created by Clearwater.
 * */

namespace UltraBINGO;

[BepInPlugin(PluginId, PluginName, PluginVersion)]
[BepInDependency("com.eternalUnion.angryLevelLoader")]
public class Main : BaseUnityPlugin {
    private const string PluginId = "clearwater.ultrakillbingo.ultrakillbingo";
    private const string PluginName = "Baphomet's BINGO";
    public const string PluginVersion = "1.1.1";

    public static string? ModFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public const bool IsDevelopmentBuild = false;
    public static bool IsSteamAuthenticated;
    public static bool HasUnlocked = true;
    public static bool UpdateAvailable = false;

    private static readonly List<string> LoadedMods = [];

    // Mod init logic
    private void Awake() {
        Logging.LoadStep("Now loading Baphomet's Bingo...");

        Debug.unityLogger.filterLogType = LogType.Warning;

        Logging.LoadStep("Loading packets...");

        PacketLoader.Load();

        Logging.LoadStep("Loading asset bundle...");

        AssetLoader.LoadAssets();

        Logging.LoadStep("Applying patches...");

        var harmony = new Harmony(PluginId);

        harmony.PatchAll();

        Logging.LoadStep("Initializing the network manager...");

        NetworkManager.serverURLConfig =
            Config.Bind("ServerConfig", "serverUrl", "clearwaterbirb.uk", "Server URL");

        NetworkManager.serverPortConfig = Config.Bind("ServerConfig", "serverPort", "2052", "Server Port");

        NetworkManager.lastRankUsedConfig = Config.Bind(
            "ServerConfig",
            "lastRankUsed",
            "None",
            "Last Rank Used (Only works if your SteamID has access to this rank)"
        );

        NetworkManager.Initialise(NetworkManager.serverURLConfig.Value, NetworkManager.serverPortConfig.Value);

        Logging.LoadStep("Done!");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //Make sure the client is running a legit copy of the game
    private static void Authenticate() {
        Logging.Message("Authenticating game ownership with Steam...");
        try {
            var ticket = SteamUser.GetAuthSessionTicket(new NetIdentity());
            var ticketString = BitConverter.ToString(ticket.Data, 0, ticket.Data.Length).Replace("-", string.Empty);

            if (ticketString.Length <= 0) return;

            IsSteamAuthenticated = true;
            NetworkManager.SetSteamTicket(ticketString);
        } catch (Exception) {
            Logging.Error("Unable to authenticate with Steam!");
        }
    }

    public void VerifyModWhitelist() {
        foreach (var modData in
                 Chainloader.PluginInfos.Select(plugin => plugin.Value.ToString().Split(' ').ToList())) {
            modData.RemoveAt(modData.Count - 1);
            var modName = string.Join(" ", modData);
            LoadedMods.Add(modName);
        }

        var vmr = new VerifyModRequest {
            ClientModList = LoadedMods,
            SteamId = SteamClient.SteamId.ToString()
        };

        NetworkManager.SendModCheck(vmr);
    }


    //Scene switch
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        GameManager.ResetVars();

        if (GetSceneName() == "Main Menu") {
            HasUnlocked = HasUnlockedMod();
            if (!IsSteamAuthenticated) {
                Authenticate();
                VerifyModWhitelist();
            }

            if (GameManager.currentSetGame?.IsGameFinished() ?? false) {
                BingoEnd.ShowEndScreen();
                MonoSingleton<AssistController>.Instance.majorEnabled = false;
                MonoSingleton<AssistController>.Instance.gameSpeed = 1f;
            }

            UIManager.ultrabingoLockedPanel = Instantiate(
                AssetLoader.BingoLockedPanel,
                GetGameObjectChild(GetInactiveRootObject("Canvas"), "Difficulty Select (1)")?.transform
            );
            UIManager.ultrabingoUnallowedModsPanel = Instantiate(
                AssetLoader.BingoUnallowedModsPanel,
                GetGameObjectChild(GetInactiveRootObject("Canvas"), "Difficulty Select (1)")?.transform
            );

            var num = GetGameObjectChild(BingoMainMenu.VersionInfo, "VersionNum");

            if (num != null)
                num.GetComponent<TextMeshProUGUI>().text =
                    PluginVersion;

            UIManager.ultrabingoLockedPanel?.SetActive(false);
            UIManager.ultrabingoUnallowedModsPanel?.SetActive(false);
        } else {
            UIManager.RemoveLimit();

            if (!GameManager.IsInBingoLevel) return;

            if (GameManager.currentSetGame?.GameSettings.DisableCampaignAltExits ?? false)
                CampaignPatches.Apply(GetSceneName());
        }
    }
}