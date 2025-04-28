using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UltraBINGO.Components;

public class BingoLevelSquare : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    BingoLevelData levelData;
    
    bool isHovered = false;
    
    public void OnPointerEnter(PointerEventData data)
    {
        isHovered = true;
    }
    
    public void OnPointerExit(PointerEventData data)
    {
        isHovered = false;
    }
    
    public void Start()
    {
        levelData = GetComponentInParent<BingoLevelData>();
    }
    
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.R) && isHovered && !MonoSingleton<BingoVoteManager>.Instance.voteOngoing)
        {
            //Make sure the level isn't already claimed
            if(!levelData.isClaimed)
            {
                GameManager.RequestReroll(levelData.row,levelData.column);
            }
        }
    }
    
    public void OnPointerClick(PointerEventData data)
    {
        switch(data.button)
        {
            case PointerEventData.InputButton.Left:
            {
                // Go to map
                BingoMenuController.LoadBingoLevelFromPauseMenu(gameObject.name,gameObject.GetComponent<BingoLevelData>());
                break;
            }
            case PointerEventData.InputButton.Right:
            {
                // Ping map
                GameManager.PingMapForTeam(GameManager.CurrentTeam,levelData.row,levelData.column);
                break;
            }
            default: {break;}
        }
    }
}