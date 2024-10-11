using System.IO;
using TMPro;
using UltrakillBingoClient;
using UnityEngine;

namespace UltraBINGO;

public static class AssetLoader
{
    public static GameObject ultrabingoMenu;
    public static TMP_FontAsset gameFont;
    public static Font gameFontLegacy;
    
    public static Sprite UISprite;
    
    public static GameObject searchBar;
    public static GameObject BingoGameSettings;
    public static GameObject BingoPauseCard;
    public static GameObject BingoTeammatesCard;
    
    public static void LoadAssets()
    {
        AssetBundle assets = AssetBundle.LoadFromFile(Path.Combine(Main.ModFolder,"bingo.resource"));
        ultrabingoMenu = assets.LoadAsset<GameObject>("UltraBingoManager");
        gameFont = assets.LoadAsset<TMP_FontAsset>("VCR_OSD_MONO_EXTENDED_TMP");
        gameFontLegacy = assets.LoadAsset<Font>("VCR_OSD_MONO_LEGACY");
        UISprite = assets.LoadAsset<Sprite>("UISprite");
        searchBar = assets.LoadAsset<GameObject>("IdInput");
        BingoGameSettings = assets.LoadAsset<GameObject>("BingoGameSettings");
        BingoPauseCard = assets.LoadAsset<GameObject>("BingoPauseCard");
        BingoTeammatesCard = assets.LoadAsset<GameObject>("BingoTeammateCard");
    }
}