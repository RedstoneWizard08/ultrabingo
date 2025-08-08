using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace UltraBINGO.Components;

public class BingoMenuManager : MonoBehaviour {
    private UnityEvent tryResetUI = new();

    public void OnEnable() {
        tryResetUI.AddListener(resetUI);
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.F10)) tryResetUI.Invoke();
    }

    public async void resetUI() {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Resetting bingo UI...");
        gameObject.SetActive(false);
        await Task.Delay(1000);
        gameObject.SetActive(true);
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("UI reset.");
    }
}