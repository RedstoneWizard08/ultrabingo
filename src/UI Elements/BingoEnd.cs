using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoEnd
{
    public static GameObject Root;
    
    public static GameObject WinnerIndicator;
    
    public static GameObject LeaveGame;
    
    public static async void ShowEndScreen()
    { 
        await Task.Delay(250); //Give the game a moment to fully load back into the menu before displaying
        
        GetGameObjectChild(GetInactiveRootObject("Canvas"),"Main Menu (1)").SetActive(false);
        BingoEncapsulator.Root.SetActive(true);
        BingoEncapsulator.BingoMenu.SetActive(false);
        BingoEncapsulator.BingoLobbyScreen.SetActive(false);
        BingoEncapsulator.BingoEndScreen.SetActive(true);
        WinnerIndicator.GetComponent<TextMeshProUGUI>().text = "The " + GameManager.CurrentGame.winningTeam + " team has won the game!";
        Root.SetActive(true);
        
    }
    
    public static GameObject Init()
    {
        if(Root == null)
        {
            Root = new GameObject();
        }
        Root.name = "BingoEndScreen";
        
        WinnerIndicator = UIHelper.CreateText("The PLACEHOLDER team has won the game!",32,"WinnerText");
        WinnerIndicator.transform.position = new Vector3(Screen.width*0.5f, Screen.height*0.5f, 0);
        WinnerIndicator.transform.SetParent(Root.transform);
        
        LeaveGame = UIHelper.CreateButton("LEAVE GAME","UltraBingoLeave",175f,85f,24);
        LeaveGame.transform.position = new Vector3(Screen.width*0.25f, Screen.height*0.25f, 0);
        LeaveGame.transform.SetParent(Root.transform);
        LeaveGame.GetComponent<Button>().onClick.AddListener(delegate
        {
            GameManager.LeaveGame();
        });
        LeaveGame.transform.SetParent(Root.transform);
        
    
        Root.SetActive(false);    
        return Root;
        
    }
}