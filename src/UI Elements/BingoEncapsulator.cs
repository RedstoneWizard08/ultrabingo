using UnityEngine;
using UnityEngine.UI;

namespace UltraBINGO.UI_Elements;

public static class BingoEncapsulator
{
    public static GameObject background;
    
    public static GameObject Root;
    public static GameObject BingoMenu;
    public static GameObject BingoLobbyScreen;
    public static GameObject BingoCardScreen;
    public static GameObject BingoEndScreen;
    
    
    public static GameObject Init()
    {
        if(Root == null)
        {
            Root = new GameObject();
        }
        Root.name = "UltraBingo";
        
        background = new GameObject();
        background.transform.SetParent(Root.transform);
        background.name = "Background";
        background.AddComponent<Image>();
        background.GetComponent<Image>().color = new Vector4(0,0,0,0.33f);
        background.transform.position = new Vector3(960f,540f,0f);
        background.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width,Screen.height);
        
        
        BingoMenu = BingoMainMenu.Init();
        BingoMenu.transform.SetParent(Root.transform);
        
        BingoLobbyScreen = BingoLobby.Init();
        BingoLobbyScreen.transform.SetParent(Root.transform);
        
        BingoCardScreen = BingoCard.Init();
        BingoCardScreen.transform.SetParent(Root.transform);
        
        BingoEndScreen = BingoEnd.Init();
        BingoEndScreen.transform.SetParent(Root.transform);
            

        
        return Root;
    }
}