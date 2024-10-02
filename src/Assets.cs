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
    
    public static void LoadAssets()
    {
        AssetBundle assets = AssetBundle.LoadFromFile(Path.Combine(Main.ModFolder,"ultrabingo.resource"));
        ultrabingoMenu = assets.LoadAsset<GameObject>("UltraBingoManager");
        gameFont = assets.LoadAsset<TMP_FontAsset>("VCR_OSD_MONO_EXTENDED_TMP");
        gameFontLegacy = assets.LoadAsset<Font>("VCR_OSD_MONO_LEGACY");
        Logging.Message(gameFontLegacy.ToString());
        UISprite = assets.LoadAsset<Sprite>("UISprite");
        searchBar = assets.LoadAsset<GameObject>("IdInput");
        BingoGameSettings = assets.LoadAsset<GameObject>("BingoGameSettings");
    }
}