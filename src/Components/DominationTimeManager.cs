using System;
using TMPro;
using UnityEngine;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.Components;

public class DominationTimeManager : MonoBehaviour
{
    public static GameObject Panel;
    
    public static TextMeshProUGUI Timer;
    public float timeRemaining;
    
    public bool started = false;
    
    public void Bind(GameObject source)
    { 
        Panel = source;
    }
    
    public void Start()
    {
        Timer = GetGameObjectChild(Panel,"Timer").GetComponent<TextMeshProUGUI>();
        timeRemaining = GameManager.dominationTimer;
    }

    public void Update()
    {
        timeRemaining = Mathf.MoveTowards(timeRemaining,0f,Time.unscaledDeltaTime);
        GameManager.dominationTimer = timeRemaining;
        Timer.text = "<color=orange>"+(int)timeRemaining+"</color>s";
    }
    
}