using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UltraBINGO.Components;

public class BingoLevelSquare : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
}