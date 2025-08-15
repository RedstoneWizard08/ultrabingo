using UltraBINGO.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UltraBINGO.Components;

public class BingoLevelSquare : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    private BingoLevelData? _levelData;
    private bool _isHovered;

    public void OnPointerEnter(PointerEventData data) {
        _isHovered = true;
    }

    public void OnPointerExit(PointerEventData data) {
        _isHovered = false;
    }

    public void Start() {
        _levelData = GetComponentInParent<BingoLevelData>();
    }

    public void Update() {
        if (!Input.GetKeyDown(KeyCode.R) || !_isHovered || MonoSingleton<BingoVoteManager>.Instance.voteOngoing) return;

        //Make sure the level isn't already claimed
        if (_levelData?.IsClaimed == false)
            GameManager.RequestReroll(_levelData.Row, _levelData.Column).Wait();
    }

    public void OnPointerClick(PointerEventData data) {
        switch (data.button) {
            case PointerEventData.InputButton.Left:
                // Go to map
                BingoMenuController.LoadBingoLevelFromPauseMenu(
                    gameObject.name,
                    gameObject.GetComponent<BingoLevelData>()
                );
                break;
            
            case PointerEventData.InputButton.Right:
                // Ping map
                if (_levelData != null)
                    GameManager.PingMapForTeam(GameManager.CurrentTeam, _levelData.Row, _levelData.Column).Wait();
                
                break;
            
            case PointerEventData.InputButton.Middle:
            default:
                break;
        }
    }
}