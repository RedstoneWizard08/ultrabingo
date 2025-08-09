using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace UltraBINGO.Components;

public class BingoMenuManager : MonoBehaviour {
    private readonly UnityEvent _tryResetUI = new();
    
    public void OnEnable() {
        _tryResetUI.AddListener(ResetUI);
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.F10)) _tryResetUI.Invoke();
    }

    public void ResetUI() {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Resetting bingo UI...");
        
        gameObject.SetActive(false);
        
        Task.Delay(1000).Wait();
        
        gameObject.SetActive(true);
        
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("UI reset.");
    }
}