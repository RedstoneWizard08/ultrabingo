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
    
    public static string team = "PLACEHOLDER";
    
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

        return Root;
    }
}