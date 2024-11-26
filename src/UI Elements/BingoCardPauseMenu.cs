using System.Collections.Generic;
using TMPro;
using UltraBINGO.Components;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

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
    
    public static GameObject BingoPauseCard;
    
    public static GameObject LevelSquareTemplate;
    
    
    
    public static void onMouseEnterLevelSquare(PointerEventData data)
    {
        string angryLevelName = data.pointerEnter.gameObject.GetComponent<BingoLevelData>().levelName.ToLower();
        string campaignLevelName = GameManager.CurrentGame.grid.levelTable[data.pointerEnter.gameObject.name].levelId.ToLower();
        
        string path = "assets/bingo/lvlimg/" + (data.pointerEnter.gameObject.GetComponent<BingoLevelData>().isAngryLevel ? "angry" : "campaign") + "/"
            + (data.pointerEnter.gameObject.GetComponent<BingoLevelData>().isAngryLevel ? angryLevelName : campaignLevelName) + ".png";
        
        Texture2D levelImg = AssetLoader.Assets.LoadAsset<Texture2D>(path);    
        Sprite levelSprite = Sprite.Create(levelImg, new Rect(0.0f, 0.0f, levelImg.width, levelImg.height), new Vector2(0.5f, 0.5f), 100.0f);
        
        GetGameObjectChild(Root,"SelectedLevelImage").GetComponent<Image>().overrideSprite = levelSprite;
        GetGameObjectChild(GetGameObjectChild(Root,"SelectedLevel"),"Text (TMP)").GetComponent<TextMeshProUGUI>().text = GameManager.CurrentGame.grid.levelTable[data.pointerEnter.gameObject.name].levelName;
        
        GetGameObjectChild(Root,"SelectedLevel").SetActive(true);
        GetGameObjectChild(Root,"SelectedLevelImage").SetActive(true);
    }
    
    public static void onMouseExitLevelSquare(PointerEventData data)
    {
        Logging.Message("Exit");
        GetGameObjectChild(Root,"SelectedLevelImage").SetActive(false);
    }
    
    public static void ShowBingoCardInPauseMenu(ref OptionsManager __instance)
    {
        Game currentGame = GameManager.CurrentGame;
        GameObject templateSquare = GetGameObjectChild(GetGameObjectChild(Root,"Card"),"Image");
        templateSquare.SetActive(false);
        
        for (int x = 0; x < currentGame.grid.size; x++)
        {
            for (int y = 0; y < currentGame.grid.size; y++)
            {
                GameObject levelSquare = GameObject.Instantiate(templateSquare,templateSquare.transform.parent.transform);
                levelSquare.name = x+"-"+y;
                GameLevel levelObject = GameManager.CurrentGame.grid.levelTable[levelSquare.name];
                
                //Set up BingoLevelData
                levelSquare.AddComponent<BingoLevelData>();
                levelSquare.GetComponent<BingoLevelData>().levelName = levelObject.levelName;
                levelSquare.GetComponent<BingoLevelData>().isAngryLevel = levelObject.isAngryLevel;
                levelSquare.GetComponent<BingoLevelData>().angryParentBundle = levelObject.angryParentBundle;
                levelSquare.GetComponent<BingoLevelData>().angryLevelId = levelObject.angryLevelId;
                
                levelSquare.AddComponent<Button>();
                levelSquare.GetComponent<Button>().onClick.AddListener(delegate
                {
                    BingoMenuController.LoadBingoLevelFromPauseMenu(levelSquare.name,levelSquare.GetComponent<BingoLevelData>());
                });
                levelSquare.GetComponent<Image>().color = teamColors[currentGame.grid.levelTable[x+"-"+y].claimedBy];
                
                if(GameManager.CurrentRow == x && GameManager.CurrentColumn == y)
                {
                    levelSquare.AddComponent<Outline>();
                    levelSquare.GetComponent<Outline>().effectColor = Color.yellow;
                    levelSquare.GetComponent<Outline>().effectDistance = new Vector2(3f,-3f);
                }
                
                levelSquare.AddComponent<EventTrigger>();
                EventTrigger.Entry mouseEnter = new EventTrigger.Entry();
                mouseEnter.eventID = EventTriggerType.PointerEnter;
                mouseEnter.callback.AddListener((data) =>
                {
                    onMouseEnterLevelSquare((PointerEventData)data);
                });
                levelSquare.GetComponent<EventTrigger>().triggers.Add(mouseEnter);
                
                EventTrigger.Entry mouseExit = new EventTrigger.Entry();
                mouseExit.eventID = EventTriggerType.PointerExit;
                mouseExit.callback.AddListener((data) =>
                {
                    onMouseExitLevelSquare((PointerEventData)data);
                });
                
                levelSquare.SetActive(true);
            }
        }
    }
    
    public static GameObject Init(ref OptionsManager __instance)
    {
        if(Root == null)
        {
            Root = GameObject.Instantiate(AssetLoader.BingoPauseCard,__instance.pauseMenu.gameObject.transform);
        }
        Root.name = "BingoPauseCard";
        GetGameObjectChild(Root,"Card").GetComponent<GridLayoutGroup>().constraintCount = GameManager.CurrentGame.grid.size;
        Root.SetActive(true);
        
        return Root;
    }
}