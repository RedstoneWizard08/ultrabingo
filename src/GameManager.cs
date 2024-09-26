using System;
using System.Collections.Generic;
using TMPro;
using UltraBINGO.Components;
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
    
    public static void ShowGameId()
    {
        BingoLobby.RoomIdDisplay.GetComponent<TextMeshProUGUI>().text = "Game ID: " + CurrentGame.gameId;
    }
    
    public static bool playerIsHost()
    {
        return Steamworks.SteamClient.SteamId.ToString() == CurrentGame.gameHost;
    }
    
    public static void RefreshPlayerList()
    {
        string players = "Players:<br>";
        foreach(Player player in CurrentGame.getPlayers())
        {
            players += player.username + "<br>";
        }
        
        BingoLobby.PlayerList.GetComponent<TextMeshProUGUI>().text = players;
    }
    
    public static void OnMouseOverLevel(PointerEventData data)
    {
        BingoCard.ShowLevelData(data.pointerEnter.GetComponent<BingoLevelData>());
    }
    
    public static void OnMouseExitLevel(PointerEventData data)
    {
        BingoCard.HideLevelData();
    }
    
    public static void SetupBingoCardAtLoad()
    {
        for(int x = 0; x < 3; x++)
        {
            for(int y = 0; y < 3; y++)
            {
                GameObject level = GameObject.Instantiate(BingoCard.ButtonTemplate,BingoCard.ButtonTemplate.transform.parent.transform);
                level.AddComponent<BingoLevelData>();
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
                
                string lvlCoords = x+"-"+y;
                level.name = lvlCoords;
                level.transform.SetParent(BingoCard.Grid.transform);
                GetGameObjectChild(level,"Text").GetComponent<TextMeshProUGUI>().text = "BingoCardButton";
                level.SetActive(true);
            }
        }
    }
    
    public static void SetupBingoCard(GameGrid grid)
    {
        for(int x = 0; x < grid.size; x++)
        {
            for(int y = 0; y < grid.size; y++)
            {
                string lvlCoords = x+"-"+y;
                GameObject lvl =  GetGameObjectChild(GetGameObjectChild(BingoCard.ButtonTemplate.transform.parent.gameObject,"BingoGrid"),lvlCoords);
                GameLevel levelObject = grid.levelTable[lvlCoords];
                GetGameObjectChild(lvl,"Text").GetComponent<TextMeshProUGUI>().text = levelObject.levelName;
                lvl.GetComponent<Button>().onClick.AddListener(delegate
                {
                    BingoMenuController.LoadBingoLevel(levelObject.levelName,lvlCoords);
                });
            }
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
        BingoLobby.RequirePRank.interactable = isHost;
        BingoLobby.GameType.interactable = isHost;
        BingoLobby.Difficulty.interactable = isHost;
        BingoLobby.StartGame.SetActive(isHost);
        
        SetupBingoCard(game.grid);
    }
    
    public static void StartGame()
    {
        NetworkManager.SendStartGameSignal(CurrentGame.gameId);
    }
    
    public static void LeaveGame()
    {
        //Send a request to the server saying we want to leave.
        NetworkManager.SendLeaveGameRequest(CurrentGame.gameId);
        
        //When we get the ok signal back, back out of the lobby/card UI.
        /*BingoEncapsulator.BingoMenu.SetActive(true);
        BingoEncapsulator.BingoLobbyScreen.SetActive(false);
        BingoEncapsulator.BingoCardScreen.SetActive(false);*/
    }
    
    public static void MoveToCard()
    {
        BingoCard.UpdateTitles();

        
        BingoEncapsulator.BingoLobbyScreen.SetActive(false);
        BingoEncapsulator.BingoCardScreen.SetActive(true);
    }
    
    public static void UpdateCards(int row, int column, string team, string playername, float newRequirement)
    {
        string coordLookup = row+"-"+column;
        
        Logging.Message(coordLookup);
        
        GameManager.CurrentGame.grid.levelTable[coordLookup].claimedBy = team;
        
        if(getSceneName() == "Main Menu")
        {
            GameObject bingoGrid = GetGameObjectChild(BingoEncapsulator.BingoCardScreen,"BingoGrid");
            
            GetGameObjectChild(bingoGrid,coordLookup).GetComponent<Image>().color = BingoCardPauseMenu.teamColors[team];
            GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().isClaimed = true;
            GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().claimedTeam = team;
            GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().claimedPlayer = playername;
            
            if(GameManager.CurrentGame.gameSettings.gameType == 0) {GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().timeRequirement = newRequirement;}
            else{ {GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().styleRequirement = newRequirement;}}
            
        }
        else
        {
            GetGameObjectChild(BingoCardPauseMenu.Root,coordLookup).GetComponent<Image>().color = BingoCardPauseMenu.teamColors[team];
        }
    }
}