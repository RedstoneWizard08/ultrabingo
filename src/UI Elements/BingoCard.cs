using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltraBINGO.UI_Elements;

public class BingoCard
{
    public static GameObject Root;
    
    public static GameObject Grid;
    
    public static GameObject ButtonTemplate;
    
    public static GameObject LeaveGame;
    
    public static GameObject TeamIndicator;
    public static GameObject ObjectiveIndicator;
    
    public static string team = "PLACEHOLDER";
    
    public static void UpdateTitles()
    {
        TeamIndicator.GetComponent<TMP_Text>().text = "-- You are on the <color=" + GameManager.currentTeam.ToLower() + ">" + GameManager.currentTeam + " team</color> --";
        ObjectiveIndicator.GetComponent<TMP_Text>().text =
            (GameManager.CurrentGame.gameSettings.gameType == 0
                ? "Race to <color=orange>obtain the fastest time</color> for your team on each level!"
                : "Rack up <color=orange>as much style</color> as you can for your team on each level!");
        
        if(GameManager.CurrentGame.gameSettings.requiresPRank)
        {
            ObjectiveIndicator.GetComponent<TMP_Text>().text += "\n <color=#ffa200d9>P</color>-Ranks are <color=orange>required</color>. You need to finish a level with a <color=#ffa200d9>P</color>-Rank to claim it.";
        }
    }
    
    public static GameObject Init()
    {
        if(Root == null)
        {
            Root = new GameObject();
        }
        Root.name = "UltraBingoCard";
        
        Grid = new GameObject();
        Grid.name = "BingoGrid";
        Grid.transform.SetParent(Root.transform);
        
        Grid.AddComponent<GridLayoutGroup>();
        Grid.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        Grid.GetComponent<GridLayoutGroup>().constraintCount = 3;
        Grid.GetComponent<GridLayoutGroup>().spacing = new Vector2(100f,100f);
        Grid.GetComponent<GridLayoutGroup>().cellSize = new Vector2(150f,50f);
        Grid.transform.position = new Vector3(Screen.width*0.35f,Screen.height*0.65f,0f);
        
        if(ButtonTemplate == null)
        {
            ButtonTemplate = new GameObject();
        }
        ButtonTemplate = UIHelper.CreateButton("LevelExample","LevelButtonTemplate",275f,25f,24);
        ButtonTemplate.transform.SetParent(Root.transform);
        ButtonTemplate.SetActive(false);
        
        // Create the grid ahead of time.
        // I don't like doing it like this but trying to instantiate buttons at runtime causes the whole game to crash.
        // So we set it up at load time.
        // TODO: Apparantly it's cause by using TextMeshProGUI instead of using TMP_Text in CreateButton/Text? Will look into.
        GameManager.SetupBingoCardAtLoad();
        
        LeaveGame = UIHelper.CreateButton("LEAVE GAME","UltraBingoLeave",175f,85f,24);
        LeaveGame.transform.position = new Vector3(Screen.width*0.25f, Screen.height*0.25f, 0);
        LeaveGame.transform.SetParent(Root.transform);
        LeaveGame.GetComponent<Button>().onClick.AddListener(delegate
        {
            GameManager.LeaveGame();
        });

        
        //Root.transform.position = new Vector3(Screen.width*0.34f, Screen.height*0.60f, 0);
        Root.SetActive(false);
        
        TeamIndicator = UIHelper.CreateText("-- You are on the "+team+" team -- ",32,"TeamIndicator");
        TeamIndicator.transform.position = new Vector3(Screen.width*0.5f,Screen.height*0.8f,0f);
        TeamIndicator.transform.SetParent(Root.transform);
        
        ObjectiveIndicator = UIHelper.CreateText("Objective here",18,"Objective Indicator");
        ObjectiveIndicator.transform.position = new Vector3(Screen.width*0.5f,Screen.height*0.75f,0f);
        ObjectiveIndicator.transform.SetParent(Root.transform);

        return Root;
    }
}