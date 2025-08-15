using System;
using TMPro;
using UnityEngine;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.Components;

public class DominationTimeManager : MonoBehaviour {
    private static GameObject? _panel;
    private static TextMeshProUGUI? _timer;
    public float timeRemaining;

    public bool started;

    public void Bind(GameObject source) {
        _panel = source;
    }

    public void Start() {
        if (GameManager.CurrentGame.GameSettings.Gamemode != 1 || gameObject == null) return;
        
        if (GameManager.IsInBingoLevel) {
            _timer = GetGameObjectChild(_panel, "Timer")?.GetComponent<TextMeshProUGUI>();
            _panel?.SetActive(false);
        }

        timeRemaining = GameManager.dominationTimer;
    }

    public void Update() {
        if (!GameManager.IsInBingoLevel || GameManager.CurrentGame.GameSettings.Gamemode != 1) return;
        
        timeRemaining = Mathf.MoveTowards(timeRemaining, 0f, Time.unscaledDeltaTime);
        GameManager.dominationTimer = timeRemaining;

        var secs = timeRemaining;
        float mins = 0;
        while (secs >= 60f) {
            secs -= 60f;
            mins += 1f;
        }

        if (!GameManager.IsInBingoLevel || _panel == null) return;
            
        if (_timer != null)
            _timer.text =
                $"<color=orange>{mins}:{(secs < 10f ? ((int)secs).ToString("D2") : (int)secs)}</color>";
                
        if (MonoSingleton<InputManager>.Instance.InputSource.Stats.WasPerformedThisFrame ||
            PlayerPrefs.GetInt("LevStaOpe") == 1)
            _panel.SetActive(true);
        else if (MonoSingleton<InputManager>.Instance.InputSource.Stats.WasCanceledThisFrame)
            _panel.SetActive(false);
    }
}