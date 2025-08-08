using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UltraBINGO.Components;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;
using Object = UnityEngine.Object;

namespace UltraBINGO;

public static class UIManager {
    private static GameObject? _ultrabingoButtonObject;
    private static GameObject? _ultrabingoEncapsulator;
    public static GameObject? ultrabingoLockedPanel = null;
    public static GameObject? ultrabingoUnallowedModsPanel = null;

    private static bool _wasVsyncActive;
    private static int _fpsLimit = -1;

    public static List<string> nonWhitelistedMods = [];

    public static async Task HandleGameSettingsUpdate() {
        //Only send if we're the host.
        if (!GameManager.PlayerIsHost()) return;

        var updateReq = new UpdateRoomSettingsRequest {
            roomId = GameManager.CurrentGame.gameId,
            maxPlayers = int.Parse(BingoLobby.MaxPlayers?.text ?? "8"),
            maxTeams = int.Parse(BingoLobby.MaxTeams?.text ?? "4"),
            timeLimit = int.Parse(BingoLobby.TimeLimit?.text ?? "5"),
            gamemode = BingoLobby.Gamemode?.value ?? 0,
            teamComposition = BingoLobby.TeamComposition?.value ?? 0,
            PRankRequired = BingoLobby.RequirePRank?.isOn ?? false,
            difficulty = BingoLobby.Difficulty?.value ?? 2,
            gridSize = BingoLobby.GridSize?.value ?? 3,
            disableCampaignAltExits = BingoLobby.DisableCampaignAltExits?.isOn ?? false,
            gameVisibility = BingoLobby.GameVisibility?.value ?? 0,
            ticket = NetworkManager.CreateRegisterTicket()
        };

        await NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(updateReq));
    }

    public static void SetupElements(CanvasController controller) {
        var canvasRectTransform = controller.GetComponent<RectTransform>();
        var difficultySelectObject = canvasRectTransform.Find("Difficulty Select (1)").gameObject;

        if (_ultrabingoButtonObject == null) {
            _ultrabingoButtonObject =
                Object.Instantiate(AssetLoader.BingoEntryButton, difficultySelectObject.transform);
            if (_ultrabingoButtonObject != null) _ultrabingoButtonObject.name = "UltraBingoButton";
        }

        var bingoButton = _ultrabingoButtonObject?.GetComponent<Button>();

        bingoButton?.onClick.AddListener(Open);

        if (_ultrabingoEncapsulator == null) {
            _ultrabingoEncapsulator = BingoEncapsulator.Init();
            _ultrabingoEncapsulator.name = "UltraBingo";
            _ultrabingoEncapsulator.transform.parent = controller.transform;
            _ultrabingoEncapsulator.transform.localPosition = Vector3.zero;
            _ultrabingoEncapsulator.AddComponent<BingoMenuManager>();
        }

        _ultrabingoEncapsulator.SetActive(false);
    }

    private static void PopulateUnallowedMods() {
        var mods = GetGameObjectChild(
            GetGameObjectChild(GetGameObjectChild(ultrabingoUnallowedModsPanel, "BingoLockedPanel"), "Panel"),
            "ModList")?.GetComponent<TextMeshProUGUI>();

        var text = nonWhitelistedMods.Aggregate("<color=orange>", (current, mod) => current + (mod + "\n"));

        text += "</color>";

        if (mods != null) mods.text = text;
    }

    private static void EnforceLimit() {
        _wasVsyncActive = QualitySettings.vSyncCount == 1;
        _fpsLimit = Application.targetFrameRate;

        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
    }

    public static void RemoveLimit() {
        Application.targetFrameRate = _fpsLimit;
        QualitySettings.vSyncCount = _wasVsyncActive ? 1 : 0;
    }

    private static void Open() {
        if (!NetworkManager.modlistCheckDone) {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                "Mod check failed, please restart your game.\nIf this keeps happening, please check your internet.");
            return;
        }

        if (!NetworkManager.modlistCheckPassed) {
            PopulateUnallowedMods();
            ultrabingoUnallowedModsPanel?.SetActive(true);
            return;
        }

        if (Main.HasUnlocked) {
            if (NetworkManager.IsConnectionUp()) {
                NetworkManager.DisconnectWebSocket();
                GameManager.ClearGameVariables();
            }

            //Enforce FPS and VSync lock to minimize crash/freezing from UI elements.
            EnforceLimit();

            //Hide chapter select
            _ultrabingoButtonObject?.transform.parent.gameObject.SetActive(false);

            BingoEncapsulator.BingoLobbyScreen?.SetActive(false);
            BingoEncapsulator.Root?.SetActive(true);
            BingoEncapsulator.BingoMenu?.SetActive(true);

            NetworkManager.SetState(State.INMENU);
        } else {
            //Show locked panel
            ultrabingoLockedPanel?.SetActive(true);
        }
    }
}