using Newtonsoft.Json;
using TMPro;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO;

public static class UIManager
{
    public static GameObject ultrabingoButtonObject = null;
    public static GameObject ultrabingoEncapsulator = null;
    
    public static void HandleGameSettingsUpdate()
    {
        //Only send if we're the host.
        if(GameManager.playerIsHost())
        {
            UpdateRoomSettingsRequest urss = new UpdateRoomSettingsRequest();
            urss.roomId = GameManager.CurrentGame.gameId;
            urss.maxPlayers = int.Parse(BingoLobby.MaxPlayers.text);
            urss.maxTeams = int.Parse(BingoLobby.MaxTeams.text);
            urss.PRankRequired = BingoLobby.RequirePRank.isOn;
            urss.gameType = BingoLobby.GameType.value;
            urss.difficulty = BingoLobby.Difficulty.value;
            urss.levelRotation = BingoLobby.LevelSelection.value;
            urss.gridSize = BingoLobby.GridSize.value;
        
            NetworkManager.sendEncodedMessage(JsonConvert.SerializeObject(urss));
        }

    }
    
    public static void Open()
    {
        
        //Hide chapter select
        ultrabingoButtonObject.transform.parent.gameObject.SetActive(false);
        BingoEncapsulator.Root.SetActive(true);
        
    }
    
    public static void Close()
    {
        //Show chapter select
        ultrabingoEncapsulator.SetActive(false);
        ultrabingoButtonObject.transform.parent.gameObject.SetActive(true);
    }
    
    
    //Borrowed and repurposed this code from Hydra's BossRush mod, cheers man :D
    public static void SetupElements(CanvasController __instance)
    {
        RectTransform canvasRectTransform = __instance.GetComponent<RectTransform>();
        GameObject chapterSelectObject = canvasRectTransform.Find("Chapter Select").gameObject;
        GameObject difficultySelectObject = canvasRectTransform.Find("Difficulty Select (1)").gameObject;
        if (chapterSelectObject == null)
        {
            Logging.Error("Chapter Select object is null");
            return;
        }
        
        RectTransform chapterSelectRectTransform = chapterSelectObject.GetComponent<RectTransform>();
        GameObject sandboxButtonObject = chapterSelectObject.transform.Find("Sandbox").gameObject;

        if (sandboxButtonObject == null)
        {
            Logging.Error("Sandbox button is null");
            return;
        }
        if(ultrabingoButtonObject == null)
        {
            ultrabingoButtonObject = GameObject.Instantiate(sandboxButtonObject, difficultySelectObject.transform);
            ultrabingoButtonObject.name = "UltraBingo Button";
        }
        Button sandboxButton = ultrabingoButtonObject.GetComponent<Button>();

        ColorBlock oldColorBlock = sandboxButton.colors;
        //Have to destroy old button component because of Unity's persistent listener calls.
        //They can't be removed at runtime so the component must be replaced.
        GameObject.DestroyImmediate(sandboxButton);

        Button ultrabingoButton = ultrabingoButtonObject.AddComponent<Button>();
        ultrabingoButton.colors = oldColorBlock;
        
        RectTransform ultrabingoButtonRectTransform = ultrabingoButtonObject.GetComponent<RectTransform>();

        Vector3 buttonPosition = ultrabingoButtonRectTransform.position;
        buttonPosition.y = 250;
        ultrabingoButtonRectTransform.position = buttonPosition;

        ultrabingoButtonRectTransform.GetComponentInChildren<TextMeshProUGUI>().text = "BAPHOMET'S BINGO";
        ultrabingoButton.onClick.AddListener(Open);
        
        if(ultrabingoEncapsulator == null)
        {
            ultrabingoEncapsulator = BingoEncapsulator.Init();
            ultrabingoEncapsulator.name = "UltraBingo";
        }

        ultrabingoEncapsulator.transform.parent = __instance.transform;
        ultrabingoEncapsulator.SetActive(false);
    }
    
    public static void DisableMajorAssists(GameObject canvas)
    {
        GameObject assistsList = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(canvas,"OptionsMenu"),"Assist Options"),"Scroll Rect"),"Contents");
        
        GetGameObjectChild(assistsList,"Text (6)").SetActive(false);
        GetGameObjectChild(assistsList,"Major Assists").SetActive(false);
        
        GameObject disabledNotification = UIHelper.CreateText("Major assists are <color=orange>disabled</color> while playing Baphomet's Bingo.",26,"TextDisabled");
        disabledNotification.transform.SetParent(assistsList.transform);
    }
}