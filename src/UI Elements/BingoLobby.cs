using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UltraBINGO.NetworkMessages;
using UnityEngine;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoLobby 
{
    public static GameObject Root;
    public static GameObject PlayerList;
    public static GameObject ReturnToBingoMenu;
    public static GameObject StartGame;
    public static GameObject RoomIdDisplay;
    
    public static GameObject GameOptions;
    public static TMP_InputField MaxPlayers;
    public static TMP_InputField MaxTeams;
    public static TMP_Dropdown GridSize;
    public static TMP_Dropdown GameType;
    public static TMP_Dropdown Difficulty;
    public static TMP_Dropdown LevelSelection;
    public static Toggle RequirePRank;
    
    public static void onMaxPlayerUpdate(string playerAmount)
    {
        int amount = int.Parse(playerAmount);
        GameManager.CurrentGame.gameSettings.maxPlayers = amount;
        
        MaxPlayers.text = Mathf.Clamp(amount,2f,8f).ToString();
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onMaxTeamUpdate(string teamAmount)
    {
        int amount = int.Parse(teamAmount);
        GameManager.CurrentGame.gameSettings.maxTeams = amount;
        MaxTeams.text = Mathf.Clamp(amount,2f,8f).ToString();
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onGridSizeUpdate(int value)
    {
        GridSize.value = value;
        GameManager.CurrentGame.gameSettings.gridSize = value;
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onGameTypeUpdate(int value)
    {
        GameType.value = value;
        GameManager.CurrentGame.gameSettings.gameType = value;
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onDifficultyUpdate(int value)
    {
        GameManager.CurrentGame.gameSettings.difficulty = value;
        Difficulty.value = value;
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onLevelSelectionUpdate(int value)
    {
        GameManager.CurrentGame.gameSettings.levelRotation = value;
        LevelSelection.value = value;
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onPRankRequiredUpdate(bool value)
    {
        RequirePRank.isOn = value;
        GameManager.CurrentGame.gameSettings.requiresPRank = value;
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void updateFromNotification(UpdateRoomSettingsNotification newSettings)
    {
        MaxPlayers.text = newSettings.maxPlayers.ToString();
        MaxTeams.text = newSettings.maxTeams.ToString();
        RequirePRank.isOn = newSettings.PRankRequired;
        GameType.value = newSettings.gameType;
        Difficulty.value = newSettings.difficulty;
        LevelSelection.value = newSettings.levelRotation;
        GridSize.value = newSettings.gridSize;
        
        GameManager.CurrentGame.gameSettings.maxPlayers = newSettings.maxPlayers;
        GameManager.CurrentGame.gameSettings.maxTeams = newSettings.maxTeams;
        GameManager.CurrentGame.gameSettings.requiresPRank = newSettings.PRankRequired;
        GameManager.CurrentGame.gameSettings.gameType = newSettings.gameType;
        GameManager.CurrentGame.gameSettings.difficulty = newSettings.difficulty;
        GameManager.CurrentGame.gameSettings.levelRotation = newSettings.levelRotation;
        GameManager.CurrentGame.gameSettings.gridSize = newSettings.gridSize;
    }
    
    public static GameObject Init()
    {
        if(Root == null)
        {
            Root = new GameObject();
        }
        Root.name = "UltraBingoLobby";
        
        //Player list
        PlayerList = UIHelper.CreateText("Players:",18,"PlayerList");
        PlayerList.transform.position = new Vector3(Screen.width*0.25f, Screen.height*0.75f, 0);
        PlayerList.transform.SetParent(Root.transform);
        
        //Leave game button
        ReturnToBingoMenu = UIHelper.CreateButton("LEAVE GAME","UltraBingoLeave",175f,85f,24);
        ReturnToBingoMenu.transform.position = new Vector3(Screen.width*0.25f, Screen.height*0.25f, 0);
        ReturnToBingoMenu.transform.SetParent(Root.transform);
        ReturnToBingoMenu.GetComponent<Button>().onClick.AddListener(delegate
        {
            GameManager.LeaveGame();
        });
        
        //Start game button
        StartGame = UIHelper.CreateButton("START GAME","UltraBingoStart",175f,85f,24);
        StartGame.transform.position = new Vector3(Screen.width*0.75f, Screen.height*0.25f, 0);
        StartGame.transform.SetParent(Root.transform);
        StartGame.GetComponent<Button>().onClick.AddListener(delegate
        {
            GameManager.StartGame();
        });
        
        //Room id text
        RoomIdDisplay = UIHelper.CreateText("Room ID:",18,"RoomId");
        RoomIdDisplay.transform.position = new Vector3(Screen.width*0.75f, Screen.height*0.75f, 0);
        RoomIdDisplay.transform.SetParent(Root.transform);
        
        //Game options
        GameOptions = GameObject.Instantiate(AssetLoader.BingoGameSettings,Root.transform);
        GameOptions.name = "BingoGameSettings";
        GameOptions.transform.position = new Vector3(Screen.width*0.40f,Screen.height*0.70f,0f);
        
        MaxPlayers = GetGameObjectChild(GetGameObjectChild(GameOptions,"MaxPlayers"),"Input").GetComponent<TMP_InputField>();
        MaxPlayers.onEndEdit.AddListener(onMaxPlayerUpdate);
        
        MaxTeams = GetGameObjectChild(GetGameObjectChild(GameOptions,"MaxTeams"),"Input").GetComponent<TMP_InputField>();
        MaxTeams.onEndEdit.AddListener(onMaxTeamUpdate);
        
        GridSize = GetGameObjectChild(GetGameObjectChild(GameOptions,"GridSize"),"Dropdown").GetComponent<TMP_Dropdown>();
        GridSize.onValueChanged.AddListener(onGridSizeUpdate);
        
        GameType = GetGameObjectChild(GetGameObjectChild(GameOptions,"GameType"),"Dropdown").GetComponent<TMP_Dropdown>();
        GameType.onValueChanged.AddListener(onGameTypeUpdate);
        
        Difficulty = GetGameObjectChild(GetGameObjectChild(GameOptions,"Difficulty"),"Dropdown").GetComponent<TMP_Dropdown>();
        Difficulty.onValueChanged.AddListener(onDifficultyUpdate);
        
        LevelSelection = GetGameObjectChild(GetGameObjectChild(GameOptions,"CustomLevels"),"Dropdown").GetComponent<TMP_Dropdown>();
        LevelSelection.onValueChanged.AddListener(onLevelSelectionUpdate);
        
        RequirePRank = GetGameObjectChild(GetGameObjectChild(GameOptions,"RequirePRank"),"Input").GetComponent<Toggle>();
        RequirePRank.onValueChanged.AddListener(onPRankRequiredUpdate);
        
        Root.SetActive(false);
        return Root;
    }
}