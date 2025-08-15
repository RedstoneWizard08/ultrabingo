using System;
using TMPro;
using UnityEngine;

namespace UltraBINGO.Util;

public static class AssetLoader {
    private const string BundlePath = "bingo.assets";

    public static AssetBundle Assets = null!;

    public static TMP_FontAsset GameFont = null!;
    public static Font GameFontLegacy = null!;
    public static Sprite UISprite = null!;
    public static GameObject BingoEntryButton = null!;
    public static GameObject BingoPauseCard = null!;
    public static GameObject BingoMainMenu = null!;
    public static GameObject BingoLobbyMenu = null!;
    public static GameObject BingoCardElements = null!;
    public static GameObject BingoEndScreen = null!;
    public static GameObject BingoSetTeams = null!;
    public static GameObject BingoMapSelectionMenu = null!;
    public static GameObject BingoCardButtonTemplate = null!;
    public static GameObject BingoGameBrowser = null!;
    public static GameObject BingoVotePanel = null!;
    public static GameObject BingoDominationTimer = null!;
    public static GameObject BingoInGameGridPanel = null!;
    public static GameObject BingoLockedPanel = null!;
    public static GameObject BingoUnallowedModsPanel = null!;
    public static AudioClip GameOverSound = null!;

    private static AssetBundle? LoadBundle() => BundleUtils.LoadEmbeddedAssetBundle(
        typeof(Main).Assembly,
        $"{typeof(Main).Namespace}.{BundlePath}"
    );

    public static void LoadAssets() {
        Assets = LoadBundle() ?? throw new InvalidOperationException("Failed to load asset bundle!");

        GameFont = Assets.LoadAsset<TMP_FontAsset>("VCR_OSD_MONO_EXTENDED_TMP");
        GameFontLegacy = Assets.LoadAsset<Font>("VCR_OSD_MONO_LEGACY");
        BingoEntryButton = Assets.LoadAsset<GameObject>("BingoEntryButton");
        BingoPauseCard = Assets.LoadAsset<GameObject>("BingoPauseCard");
        BingoMainMenu = Assets.LoadAsset<GameObject>("BingoMainMenu");
        BingoLobbyMenu = Assets.LoadAsset<GameObject>("BingoLobbyMenu");
        BingoCardElements = Assets.LoadAsset<GameObject>("BingoCard");
        BingoEndScreen = Assets.LoadAsset<GameObject>("BingoEndScreen");
        BingoSetTeams = Assets.LoadAsset<GameObject>("BingoSetTeams");
        BingoMapSelectionMenu = Assets.LoadAsset<GameObject>("BingoMapSelection");
        BingoCardButtonTemplate = Assets.LoadAsset<GameObject>("BingoGridButtonTemplate");
        BingoGameBrowser = Assets.LoadAsset<GameObject>("BingoMatchBrowser");
        BingoLockedPanel = Assets.LoadAsset<GameObject>("BingoLocked");
        BingoUnallowedModsPanel = Assets.LoadAsset<GameObject>("BingoUnallowedMods");
        BingoInGameGridPanel = Assets.LoadAsset<GameObject>("BingoInGameGrid");
        BingoDominationTimer = Assets.LoadAsset<GameObject>("BingoDominationTimer");
        UISprite = Assets.LoadAsset<Sprite>("UISprite");
        GameOverSound = Assets.LoadAsset<AudioClip>("gameEnd");
        BingoVotePanel = Assets.LoadAsset<GameObject>("BingoVotePanel");
    }
}