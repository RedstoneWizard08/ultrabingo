using TMPro;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoMainMenu
{
    public static GameObject Root;
    public static GameObject HostGame;
    public static GameObject JoinGame;
    public static GameObject JoinGameInput;
    public static GameObject Back;
    
    public static GameObject MapCheck;
    public static GameObject MapWarn;
    public static GameObject MissingMapsList;
    
    public static void Open()
    {
        //Hide chapter select
        Root.transform.parent.gameObject.SetActive(false);
        BingoEncapsulator.Root.SetActive(true);
        Root.SetActive(true);
    }
    
    public static void Close()
    {
        //Show chapter select
        BingoEncapsulator.Root.SetActive(false);
        Root.SetActive(false);
        Root.transform.parent.parent.gameObject.SetActive(true);
    }
    
    public static GameObject Init(ref GameObject BingoMenu)
    {
        
        HostGame = GetGameObjectChild(BingoMenu,"Host Game");
        HostGame.GetComponent<Button>().onClick.AddListener(BingoMenuController.CreateRoom);
        
        JoinGame = GetGameObjectChild(BingoMenu,"Join Game");
        JoinGame.GetComponent<Button>().onClick.AddListener(delegate
        {
            GameObject input = GetGameObjectChild(JoinGameInput,"InputField (TMP)");
                
            int roomId = int.Parse(input.GetComponent<TMP_InputField>().text);
            BingoMenuController.JoinRoom(roomId);
        });
        
        JoinGameInput = GetGameObjectChild(JoinGame,"IdInput");
        
        MapCheck = GetGameObjectChild(BingoMenu,"MapCheck");
        MapCheck.GetComponent<Button>().onClick.AddListener(delegate
        {
            MapWarn.SetActive(true);
        });
        
        
        MapWarn = GetGameObjectChild(BingoMenu,"MapWarn");
        MapWarn.SetActive(false);
        MissingMapsList = GetGameObjectChild(GetGameObjectChild(MapWarn,"Panel"),"MissingMapList");
        
        Back = GetGameObjectChild(BingoMenu,"Back");
        Back.GetComponent<Button>().onClick.AddListener(delegate
        {
            BingoMenuController.ReturnToMenu();
        });
        
        return Root;
    }
}