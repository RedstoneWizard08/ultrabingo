using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UltraBINGO.UI_Elements;

public static class BingoCardPauseMenu
{
    
    public static Dictionary<string,Color> teamColors = new Dictionary<string, Color>()
    {
        {"NONE",new Color(1,1 ,1,1)},
        {"Red",new Color(1,0,0,1)},
        {"Green",new Color(0,1,0,1)},
        {"Blue",new Color(0,0,1,1)}
    };


    public static GameObject Root;
    
    public static GameObject LevelSquareTemplate;
    
    public static void ShowBingoCardInPauseMenu(ref OptionsManager __instance)
    {
        Game currentGame = GameManager.CurrentGame;
        
        for (int x = 0; x < currentGame.grid.size; x++)
        {
            for (int y = 0; y < currentGame.grid.size; y++)
            {
                GameObject levelSquare = GameObject.Instantiate(LevelSquareTemplate,Root.gameObject.transform);
                levelSquare.name = x+"-"+y;
                levelSquare.AddComponent<Button>();
                levelSquare.GetComponent<Button>().onClick.AddListener(delegate
                {
                    BingoMenuController.LoadBingoLevelFromPauseMenu(levelSquare.name);
                });
                levelSquare.GetComponent<Image>().color = teamColors[currentGame.grid.levelTable[x+"-"+y].claimedBy];
                levelSquare.SetActive(true);
            }
        }
        
        //GameObject pauseMenuEncap = GameObject.Instantiate(Root,__instance.gameObject.transform);
        //pauseMenuEncap.name = "BingoPauseMenuCard";
    }
    
    public static GameObject Init(ref OptionsManager __instance)
    {
        Game currentGame = GameManager.CurrentGame;
        
        if(Root == null)
        {
            Root = new GameObject();
        }
        Root.AddComponent<GridLayoutGroup>();
        Root.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        Root.GetComponent<GridLayoutGroup>().constraintCount = 3;
        Root.GetComponent<GridLayoutGroup>().cellSize = new Vector2(40f,40f);
        Root.GetComponent<GridLayoutGroup>().spacing = new Vector2(25f,25f);
        Root.name = "BingoCardPause";
        Root.transform.SetParent(__instance.pauseMenu.gameObject.transform);
        Root.transform.localPosition = new Vector3(-225,50,0);
        
        LevelSquareTemplate = new GameObject();
        LevelSquareTemplate.AddComponent<RectTransform>();
        LevelSquareTemplate.AddComponent<Image>();
        
        LevelSquareTemplate.SetActive(false);
        Root.SetActive(true);
        
        return Root;
    }
}