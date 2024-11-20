using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UltraBINGO.Components;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO;

public static class GameManager
{
    
    public static Game CurrentGame;
    
    public static bool isInBingoLevel = false;
    public static bool returningFromBingoLevel = false;
    
    public static int currentRow = 0;
    public static int currentColumn = 0;
    
    public static string currentTeam = "";
    public static List<String> teammates;
    
    public static bool hasSent = false;
    public static bool enteringAngryLevel = false;
    
    public static bool isDownloadingLevel = false;
    public static GameObject levelBeingDownloaded = null;
    
    public static bool triedToActivateCheats = false;
    
    public static bool PreStartChecks()
    {
        //Check if the amount of selected maps is enough to fill the grid.
        int gridSize = GameManager.CurrentGame.gameSettings.gridSize+3;
        int requiredMaps = gridSize*gridSize;
        
        if(BingoMapSelection.NumOfMapsTotal < requiredMaps)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Not enough maps selected. Add more map pools, or reduce the grid size.\n" +
                                                                      "(<color=orange>" + requiredMaps + " </color>required, <color=orange>" + BingoMapSelection.NumOfMapsTotal + "</color> selected)");
            return false;
        }
        
        return true;
    }
    
    public static void ShowGameId()
    {
        GetGameObjectChild(GetGameObjectChild(BingoLobby.RoomIdDisplay,"Title"),"Text").GetComponent<Text>().text = "Game ID: " + CurrentGame.gameId;
        //BingoLobby.RoomIdDisplay.GetComponent<TextMeshProUGUI>().text = "Game ID: " + CurrentGame.gameId;
    }
    
    public static bool playerIsHost()
    {
        return Steamworks.SteamClient.SteamId.ToString() == CurrentGame.gameHost;
    }
    
    public static void RefreshPlayerList()
    {
        BingoLobby.PlayerList.SetActive(false);
        string players = "";
        foreach(Player player in CurrentGame.getPlayers())
        {
            players += player.username + "\n";
        }
        
        GetGameObjectChild(BingoLobby.PlayerList,"Players").GetComponent<TextMeshProUGUI>().text = players;
        BingoLobby.PlayerList.SetActive(true);
    }
    
    public static void OnMouseOverLevel(PointerEventData data)
    {
        BingoCard.ShowLevelData(data.pointerEnter.transform.parent.gameObject.GetComponent<BingoLevelData>());
    }
    
    public static void OnMouseExitLevel(PointerEventData data)
    {
        BingoCard.HideLevelData();
    }
    
    
    public static void SetupBingoCardDynamic()
    {
        //Resize the GridLayoutGroup based on the grid size.
        Logging.Message("Dynamic setup with size " + CurrentGame.grid.size);
        GameObject gridObj = GetGameObjectChild(BingoCard.Root,"BingoGrid");
        gridObj.GetComponent<GridLayoutGroup>().spacing = new Vector2(30,30);
        gridObj.GetComponent<GridLayoutGroup>().constraintCount = CurrentGame.grid.size;
        
        gridObj.transform.position = Vector3.zero;
        gridObj.transform.localPosition = Vector3.zero;
        switch(CurrentGame.grid.size)
        {
            case 3:
            {
                gridObj.transform.position = new Vector3(Screen.width*0.33f,Screen.height*0.66f,0f);
                break;
            }
            case 4:
            {
                gridObj.transform.position = new Vector3(Screen.width*0.25f,Screen.height*0.70f,0f);
                break;
            }
            case 5:
            {
                gridObj.transform.position = new Vector3(Screen.width*0.20f,Screen.height*0.75f,0f);
                break;
            }
            default:{break;}
        }
        
        Logging.Message("Loop");
        for(int x = 0; x < CurrentGame.grid.size; x++)
        {
            for(int y = 0; y < CurrentGame.grid.size; y++)
            {
                //Clone and set up the button and hover triggers.
                GameObject level = GameObject.Instantiate(BingoCard.ButtonTemplate,gridObj.transform);
                
                level.AddComponent<EventTrigger>();
                EventTrigger.Entry mouseEnter = new EventTrigger.Entry();
                mouseEnter.eventID = EventTriggerType.PointerEnter;
                mouseEnter.callback.AddListener((data) =>
                {
                    OnMouseOverLevel((PointerEventData)data);
                });
                level.GetComponent<EventTrigger>().triggers.Add(mouseEnter);
                
                EventTrigger.Entry mouseExit = new EventTrigger.Entry();
                mouseExit.eventID = EventTriggerType.PointerExit;
                mouseExit.callback.AddListener((data) =>
                {
                    OnMouseExitLevel((PointerEventData)data);
                });
                level.GetComponent<EventTrigger>().triggers.Add(mouseExit);
                
                //Label the button and the onclick listener.
                string lvlCoords = x+"-"+y;
                level.name = lvlCoords;
                level.transform.SetParent(BingoCard.Grid.transform);
                GetGameObjectChild(level,"Text").GetComponent<Text>().text = "BingoCardButton";
                
                GameLevel levelObject = GameManager.CurrentGame.grid.levelTable[lvlCoords];
                GetGameObjectChild(level,"Text").GetComponent<Text>().text = levelObject.levelName;
                
                //Setup the BingoLevelData component.
                level.AddComponent<BingoLevelData>();
                level.GetComponent<BingoLevelData>().isAngryLevel = levelObject.isAngryLevel;
                level.GetComponent<BingoLevelData>().angryParentBundle = levelObject.angryParentBundle;
                level.GetComponent<BingoLevelData>().angryLevelId = levelObject.angryLevelId;
                level.GetComponent<BingoLevelData>().levelName = levelObject.levelName;
                
                //Setup the click listener.
                level.GetComponent<Button>().onClick.RemoveAllListeners();
                level.GetComponent<Button>().onClick.AddListener(delegate
                {
                    BingoMenuController.LoadBingoLevel(levelObject.levelId,lvlCoords,level.GetComponent<BingoLevelData>());
                });
                
                level.SetActive(true);
            }
        }
        
        //Display teammates.
        TextMeshProUGUI teammates = GetGameObjectChild(BingoCard.Teammates,"Players").GetComponent<TextMeshProUGUI>();
        teammates.text = "";
        foreach(string player in GameManager.teammates)
        {
            teammates.text += player + "\n";
        }
    }

    public static void SetupGameDetails(Game game,bool isHost=true)
    {
        CurrentGame = game;
        
        BingoEncapsulator.BingoMenu.SetActive(false);
        BingoEncapsulator.BingoLobbyScreen.SetActive(true);
        
        ShowGameId();
        RefreshPlayerList();
        
        BingoLobby.MaxPlayers.interactable = isHost;
        BingoLobby.MaxTeams.interactable = isHost;
        BingoLobby.TeamComposition.interactable = isHost;
        BingoLobby.RequirePRank.interactable = isHost;
        BingoLobby.GameType.interactable = isHost;
        BingoLobby.Difficulty.interactable = isHost;
        BingoLobby.StartGame.SetActive(isHost);
        
        if(isHost)
        {
            //Reset field settings to default values.
            BingoLobby.MaxPlayers.text = 8.ToString();
            BingoLobby.MaxTeams.text = 4.ToString();
            BingoLobby.TeamComposition.value = 0;
            BingoLobby.GridSize.value = 0;
            BingoLobby.GameType.value = 0;
            BingoLobby.Difficulty.value = 2;
            BingoLobby.RequirePRank.isOn = false;
            
            BingoMapSelection.NumOfMapsTotal = 0;
            BingoMapSelection.UpdateNumber();
            BingoMapSelection.SelectedIds.Clear();
            foreach(GameObject mapPoolButton in BingoMapSelection.MapPoolButtons)
            {
                GetGameObjectChild(mapPoolButton,"Image").GetComponent<Image>().color = new Color(1,1,1,0);
                mapPoolButton.GetComponent<MapPoolData>().mapPoolEnabled = false;
            }
        }
        else
        {
            BingoLobby.MaxPlayers.text = CurrentGame.gameSettings.maxPlayers.ToString();
            BingoLobby.MaxTeams.text = CurrentGame.gameSettings.maxTeams.ToString();
            BingoLobby.TeamComposition.value = CurrentGame.gameSettings.teamComposition;
            BingoLobby.GridSize.value = CurrentGame.gameSettings.gridSize;
            BingoLobby.GameType.value = CurrentGame.gameSettings.gameType;
            BingoLobby.Difficulty.value = CurrentGame.gameSettings.difficulty;
            BingoLobby.RequirePRank.isOn = CurrentGame.gameSettings.requiresPRank;
        }

    }
    
    public static void StartGame()
    {
        NetworkManager.SendStartGameSignal(CurrentGame.gameId);
    }
    
    public static void LeaveGame(bool isInLevel=false)
    {
        //Send a request to the server saying we want to leave.
        NetworkManager.SendLeaveGameRequest(CurrentGame.gameId);
        
        //When that's sent off, close the connection on our end.
        NetworkManager.DisconnectWebSocket(1000,"Normal close");
        
        clearGameVariables();
        
        if(!isInLevel)
        {
            //If dc'ing from lobby/card/end screen, return to the bingo menu.
            BingoEncapsulator.BingoCardScreen.SetActive(false);
            BingoEncapsulator.BingoLobbyScreen.SetActive(false);
            BingoEncapsulator.BingoEndScreen.SetActive(false);
            BingoEncapsulator.BingoMenu.SetActive(true);
            
        }
    }
    
    public static void clearGameVariables()
    {
        CurrentGame = null;
        currentTeam = null;
        currentRow = 0;
        currentColumn = 0;
        isInBingoLevel = false;
        returningFromBingoLevel = false;
        teammates = null;
        
        //Cleanup the bingo grid if on the main menu.
        if(getSceneName() == "Main Menu")
        {
            foreach(Transform child in BingoCard.Grid.transform)
            {
                GameObject toRemove = child.gameObject;
                GameObject.Destroy(toRemove);
            }
            
        }

    }
    
    public static void MoveToCard()
    {
        BingoCard.UpdateTitles();
        
        BingoEncapsulator.BingoLobbyScreen.SetActive(false);
        BingoEncapsulator.BingoCardScreen.SetActive(true);
    }
    
    public static void UpdateCards(int row, int column, string team, string playername, float newTime, int newStyle)
    {
        string coordLookup = row+"-"+column;
        if(!CurrentGame.grid.levelTable.ContainsKey(coordLookup))
        {
            Logging.Warn("RECEIVED AN INVALID GRID POSITION TO UPDATE!");
            Logging.Warn(coordLookup);
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("A level was claimed by someone but an <color=orange>invalid grid position</color> was given.\nCheck the console and report it to Clearwater!");
            return;
        }
        GameManager.CurrentGame.grid.levelTable[coordLookup].claimedBy = team;
        GameManager.CurrentGame.grid.levelTable[coordLookup].timeToBeat = newTime;
        GameManager.CurrentGame.grid.levelTable[coordLookup].styleToBeat = newStyle;
        
        if(getSceneName() == "Main Menu")
        {
            GameObject bingoGrid = GetGameObjectChild(BingoEncapsulator.BingoCardScreen,"BingoGrid");
            
            GetGameObjectChild(bingoGrid,coordLookup).GetComponent<Image>().color = BingoCardPauseMenu.teamColors[team];
            GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().isClaimed = true;
            GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().claimedTeam = team;
            GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().claimedPlayer = playername;
            
            GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().timeRequirement = newTime;
            GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().styleRequirement = newStyle;
            
        }
        else
        {
            GetGameObjectChild(GetGameObjectChild(BingoCardPauseMenu.Root,"Card"),coordLookup).GetComponent<Image>().color = BingoCardPauseMenu.teamColors[team];
        }
    }
    
    public static void HumiliateSelf()
    {
        CheatActivation ca = new CheatActivation();
        ca.username = sanitiseUsername(Steamworks.SteamClient.Name);
        ca.gameId = CurrentGame.gameId;
        ca.steamId = Steamworks.SteamClient.SteamId.ToString();
        
        NetworkManager.sendEncodedMessage(JsonConvert.SerializeObject(ca));
    }
}