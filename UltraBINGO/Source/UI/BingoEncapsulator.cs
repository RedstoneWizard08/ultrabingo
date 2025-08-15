using UltraBINGO.Util;
using UnityEngine;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.UI;

public static class BingoEncapsulator {
    public static GameObject? Root;
    public static GameObject? BingoMenu;
    public static GameObject? BingoLobbyScreen;
    public static GameObject? BingoCardScreen;
    public static GameObject? BingoEndScreen;
    public static GameObject? BingoMapSelectionMenu;
    public static GameObject? BingoSetTeams;
    public static GameObject? BingoGameBrowser;

    public static GameObject Init() {
        if (Root == null) Root = new GameObject();
        
        Root.name = "UltraBingo";
        BingoMenu = Object.Instantiate(AssetLoader.BingoMainMenu, Root.transform);
        
        if (BingoMenu != null) {
            BingoMainMenu.Init(ref BingoMenu);
            
            BingoMenu.name = "BingoMainMenu";
            
            BingoMenu.GetOrAddComponent<MenuEsc>().previousPage =
                GetGameObjectChild(GetInactiveRootObject("Canvas"), "Difficulty Select (1)");
            
            BingoMenu.transform.SetParent(Root.transform);
        }

        BingoLobbyScreen = Object.Instantiate(AssetLoader.BingoLobbyMenu, Root.transform);
        
        if (BingoLobbyScreen != null) {
            BingoLobby.Init(ref BingoLobbyScreen);
            BingoLobbyScreen.transform.SetParent(Root.transform);
        }

        BingoCardScreen = BingoCard.Init();
        BingoCardScreen.transform.SetParent(Root.transform);

        BingoMapSelectionMenu = Object.Instantiate(AssetLoader.BingoMapSelectionMenu, Root.transform);
        
        if (BingoMapSelectionMenu != null) BingoMapSelection.Init(ref BingoMapSelectionMenu);

        BingoSetTeams = Object.Instantiate(AssetLoader.BingoSetTeams, Root.transform);
        
        if (BingoSetTeams != null) {
            BingoSetTeamsMenu.Init(ref BingoSetTeams);
            BingoSetTeams.transform.SetParent(Root.transform);
        }

        BingoEndScreen = Object.Instantiate(AssetLoader.BingoEndScreen, Root.transform);
        
        if (BingoEndScreen != null) BingoEnd.Init(ref BingoEndScreen);

        BingoGameBrowser = Object.Instantiate(AssetLoader.BingoGameBrowser, Root.transform);
        
        if (BingoGameBrowser != null) BingoBrowser.Init(ref BingoGameBrowser);

        return Root;
    }
}