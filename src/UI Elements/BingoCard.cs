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
    
    public static GameObject LevelInformation;
    public static GameObject LevelInformationText;
    
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
    
    public static void ShowLevelData(BingoLevelData levelData)
    {
        if(!levelData.isClaimed)
        {
            LevelInformationText.GetComponent<TextMeshProUGUI>().text = "This level is currently unclaimed.\n";
            LevelInformationText.GetComponent<TextMeshProUGUI>().text += (GameManager.CurrentGame.gameSettings.gameType == 0 ? "Set a time " : "Grab some style ") + "to claim it for your team!";
        }
        else
        {
            LevelInformationText.GetComponent<TextMeshProUGUI>().text = "Claimed by the <color=" + levelData.claimedTeam.ToLower()+ ">" + levelData.claimedTeam + " </color>team\n\n";
            LevelInformationText.GetComponent<TextMeshProUGUI>().text += (GameManager.CurrentGame.gameSettings.gameType == 0 ? "Time " : "Style ") + "to beat: "
                + (GameManager.CurrentGame.gameSettings.gameType == 0 ? levelData.timeRequirement : levelData.styleRequirement);
        }
        
        LevelInformation.SetActive(true);
    }
    
    public static void HideLevelData()
    {
        LevelInformation.SetActive(false);
    }
    
    public static GameObject Init()
    {
        if(Root == null)
        {
            Root = new GameObject();
        }
        Root.name = "UltraBingoCard";
        
        
        /*Grid.AddComponent<GridLayoutGroup>();
        Grid.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        Grid.GetComponent<GridLayoutGroup>().constraintCount = 3;
        Grid.GetComponent<GridLayoutGroup>().spacing = new Vector2(100f,100f);
        Grid.GetComponent<GridLayoutGroup>().cellSize = new Vector2(150f,50f);*/
        
        CardElements = GameObject.Instantiate(AssetLoader.BingoCardElements,Root.transform);
        
        //Bingo grid
        Grid = GetGameObjectChild(CardElements,"BingoGrid");
        Grid.name = "BingoGrid";
        Grid.transform.SetParent(Root.transform);
        
        if(ButtonTemplate == null)
        {
            ButtonTemplate = new GameObject();
        }
        
        //Have to create the button with normal Text instead of TextMeshProUGUI as trying to instantiate an object with the latter component causes crashes.
        ButtonTemplate = UIHelper.CreateButtonLegacy("LevelExample","LevelButtonTemplate",275f,25f,12);
        ButtonTemplate.transform.SetParent(Root.transform);
        ButtonTemplate.SetActive(false);
        
        LeaveGame = GetGameObjectChild(CardElements,"LeaveGame");
        LeaveGame.GetComponent<Button>().onClick.AddListener(delegate
        {
            GameManager.LeaveGame();
        });
        

        TeamIndicator = GetGameObjectChild(CardElements,"TeamIndicator");
        TeamIndicator.GetComponent<TextMeshProUGUI>().text = ("-- You are on the "+team+" team -- ");
        
        
        ObjectiveIndicator = GetGameObjectChild(CardElements,"ObjectiveIndicator");
        
        //Bingo Level Information panel
        LevelInformation = GetGameObjectChild(CardElements,"LevelInfo");
        LevelInformation.SetActive(false);
        
        //Level Information text
        LevelInformationText = GetGameObjectChild(LevelInformation,"Text");
        
        //Teammate panel
        Teammates = GetGameObjectChild(CardElements,"Teammates");
        
        Root.SetActive(false);
        return Root;
    }
}