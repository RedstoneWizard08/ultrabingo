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
    
    public static GameObject Init()
    {
        if(Root == null)
        {
            Root = new GameObject();
            Root.name = "UltraBingoMenu";
            
            //Host game button
            HostGame = UIHelper.CreateButton("HOST GAME","UltraBingoHost",250f,85f,38);
            HostGame.transform.position = new Vector3(Screen.width*0.5f, Screen.height*0.55f, 0);
            HostGame.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(425f, 65f);
            HostGame.GetComponentInChildren<TextMeshProUGUI>().text = "HOST GAME";
            HostGame.transform.SetParent(Root.transform);
            
            HostGame.GetComponent<Button>().onClick.AddListener(BingoMenuController.CreateRoom);
            
            // Join game button
            JoinGame = UIHelper.CreateButton("JOIN GAME","UltraBingoJoin",250f,85f,38);
            GetGameObjectChild(JoinGame,"Text").GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            JoinGame.transform.position = new Vector3(Screen.width*0.5f, Screen.height*0.45f, 0);
            JoinGame.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(425f, 65f);
            JoinGame.GetComponentInChildren<TextMeshProUGUI>().text = "JOIN GAME";
            JoinGame.transform.SetParent(Root.transform);
            
            JoinGame.GetComponent<Button>().onClick.AddListener(delegate
            {
                GameObject input = GetGameObjectChild(JoinGameInput,"InputField (TMP)");
                
                int roomId = int.Parse(input.GetComponent<TMP_InputField>().text);
                BingoMenuController.JoinRoom(roomId);
            });
            
            //Join game input field.
            JoinGameInput = GameObject.Instantiate(AssetLoader.searchBar,Root.transform);
            JoinGameInput.name = "Id Field";
            JoinGameInput.transform.position = new Vector3(Screen.width*0.45f, Screen.height*0.45f, 0);
            JoinGameInput.transform.SetParent(Root.transform);
            
            //Back button
            Back = UIHelper.CreateButton("RETURN","UltraBingoBack",125f,85f,38);
            Back.transform.position = new Vector3(Screen.width*0.25f, Screen.height*0.25f, 0);
            Back.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(200f, 65f);
            Back.GetComponentInChildren<TextMeshProUGUI>().text = "RETURN";
            Back.transform.SetParent(Root.transform);
            
            Back.GetComponent<Button>().onClick.AddListener(delegate
            {
                BingoMenuController.ReturnToMenu(Root);
            });
        }
        return Root;
    }
}