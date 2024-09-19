using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace UltraBINGO.UI_Elements;

public static class BingoLobby 
{
    public static GameObject Root;
    public static GameObject PlayerList;
    public static GameObject ReturnToBingoMenu;
    public static GameObject StartGame;
    public static GameObject RoomIdDisplay;
    
    // Stuff for the lobby:
    
    //Room id (for testing)
    //Show players currently in lobby
    //Leave game button
    
    
    public static GameObject Init()
    {
        if(Root == null)
        {
            Root = new GameObject();
        }
        Root.name = "UltraBingoLobby";
        
        
        PlayerList = UIHelper.CreateText("Players:",18,"PlayerList");
        PlayerList.transform.position = new Vector3(Screen.width*0.33f, Screen.height*0.75f, 0);
        PlayerList.transform.SetParent(Root.transform);
        
        ReturnToBingoMenu = UIHelper.CreateButton("LEAVE GAME","UltraBingoLeave",175f,85f,24);
        ReturnToBingoMenu.transform.position = new Vector3(Screen.width*0.25f, Screen.height*0.25f, 0);
        ReturnToBingoMenu.transform.SetParent(Root.transform);
        ReturnToBingoMenu.GetComponent<Button>().onClick.AddListener(delegate
        {
            GameManager.LeaveGame();
        });
        
        StartGame = UIHelper.CreateButton("START GAME","UltraBingoStart",250f,85f,18);
        StartGame.transform.position = new Vector3(Screen.width*0.75f, Screen.height*0.25f, 0);
        StartGame.transform.SetParent(Root.transform);
        StartGame.GetComponent<Button>().onClick.AddListener(delegate
        {
            GameManager.StartGame();
        });
        
        RoomIdDisplay = UIHelper.CreateText("Room ID:",18,"RoomId");
        RoomIdDisplay.transform.position = new Vector3(Screen.width*0.8f, Screen.height*0.8f, 0);
        RoomIdDisplay.transform.SetParent(Root.transform);
        
        Root.SetActive(false);
        return Root;
    }
    

}