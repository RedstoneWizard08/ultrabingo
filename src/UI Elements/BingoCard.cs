using TMPro;
using UltraBINGO.Components;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public class BingoCard
{
    public static GameObject Root;
    
    public static GameObject Grid;
    
    public static GameObject ButtonTemplate;
    
    public static GameObject LeaveGame;
    
    public static GameObject TeamIndicator;
    public static GameObject ObjectiveIndicator;
    
    public static GameObject Teammates;
    
    public static GameObject CardElements;
    
    public static string team = "PLACEHOLDER";
    
    public static void Cleanup()
    {
        foreach(Transform child in Grid.transform)
        {
            GameObject toRemove = child.gameObject;
            GameObject.Destroy(toRemove);
        }
    }
    
    public static void UpdateTitles()
    {
        TeamIndicator.GetComponent<TMP_Text>().text = "-- You are on the <color=" + GameManager.CurrentTeam.ToLower() + ">" + GameManager.CurrentTeam + " team</color> --";
        ObjectiveIndicator.GetComponent<TMP_Text>().text =
            (GameManager.CurrentGame.gameSettings.gameType == 0
                ? "Race to <color=orange>obtain the fastest time</color> for your team on each level."
                : "Rack up <color=orange>the highest style</color> as you can for your team on each level.")
            + "\nClaim " + GameManager.CurrentGame.grid.size + " levels horizontally, vertically or diagonally for your team to win!";
        
        if(GameManager.CurrentGame.gameSettings.requiresPRank)
        {
            ObjectiveIndicator.GetComponent<TMP_Text>().text += "\n<color=#ffa200d9>P</color>-Ranks are <color=orange>required</color> to claim a level.";
        }
    }
    
    public static GameObject Init()
    {
        if(Root == null)
        {
            Root = new GameObject();
        }
        Root.name = "UltraBingoCard";
        
        CardElements = GameObject.Instantiate(AssetLoader.BingoCardElements,Root.transform);
        
        //Bingo grid
        Grid = GetGameObjectChild(CardElements,"BingoGrid");
        Grid.name = "BingoGrid";
        Grid.transform.SetParent(Root.transform);
        
        if(ButtonTemplate == null)
        {
            ButtonTemplate = new GameObject();
        }
        
        //Have to create the button with normal Text instead of TextMeshProUGUI as trying to instantiate an object with the latter causes crashes.
        //ButtonTemplate = UIHelper.CreateButtonLegacy("LevelExample","LevelButtonTemplate",275f,25f,12);
        ButtonTemplate = GameObject.Instantiate(AssetLoader.BingoCardButtonTemplate,Root.transform);
        ButtonTemplate.transform.SetParent(Root.transform);
        ButtonTemplate.SetActive(false);
        
        LeaveGame = GetGameObjectChild(CardElements,"LeaveGame");
        LeaveGame.GetComponent<Button>().onClick.AddListener(delegate
        {
            GameManager.LeaveGame();
        });
        
        //Team indicator panel
        TeamIndicator = GetGameObjectChild(CardElements,"TeamIndicator");
        TeamIndicator.GetComponent<TextMeshProUGUI>().text = ("-- You are on the "+team+" team -- ");
        
        //Time/style indicator panel
        ObjectiveIndicator = GetGameObjectChild(CardElements,"ObjectiveIndicator");
        
        //Teammate panel
        Teammates = GetGameObjectChild(CardElements,"Teammates");
        
        Root.SetActive(false);
        return Root;
    }
}