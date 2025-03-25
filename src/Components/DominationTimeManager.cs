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
        if(GameManager.IsInBingoLevel)
        {
            Timer = GetGameObjectChild(Panel,"Timer").GetComponent<TextMeshProUGUI>();
        }
        
        timeRemaining = GameManager.dominationTimer;
    }

    public void Update()
    {
        timeRemaining = Mathf.MoveTowards(timeRemaining,0f,Time.unscaledDeltaTime);
        GameManager.dominationTimer = timeRemaining;
        
        float secs = timeRemaining;
        float mins = 0;
        while (secs >= 60f)
        {
            secs -= 60f;
            mins += 1f;
        }
        
        if(GameManager.IsInBingoLevel)
        {
            Timer.text = "<color=orange>"+mins+":"+(int)secs+"</color>";
        }
        
    }
    
}