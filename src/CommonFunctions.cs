using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UltraBINGO;

public static class CommonFunctions
{
    public static string getSceneName()
    {
        return SceneHelper.CurrentScene;
    }
    public static bool isOnViolentDifficulty()
    {
        return (MonoSingleton<PrefsManager>.Instance.GetInt("difficulty") == 3);
    }
     
    public static TextMeshProUGUI GetTextMeshProUGUI(GameObject objectToUse)
    {
        return objectToUse.GetComponent<TextMeshProUGUI>();
    }
    
    public static GameObject GetInactiveRootObject(string objectName)
    {
        List<GameObject> rootList = new List<GameObject>();
        SceneManager.GetActiveScene().GetRootGameObjects(rootList);
        foreach (GameObject child in rootList)
        {
            if (child.name == objectName)
            {
                return child;
            }
        }
        return null;
    }
    
    public static IEnumerator WaitforSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
    
    public static GameObject GetGameObjectChild(GameObject parentObject, string childToFind)
    {
        GameObject childToReturn = parentObject.transform.Find(childToFind).gameObject;
        return childToReturn;
    }

    public static TextMeshProUGUI getTextMeshProGUI(GameObject objectToUse)
    {
        return objectToUse.GetComponent<TextMeshProUGUI>();
    }
    
    public static Text GetTextfromGameObject(GameObject objectToUse)
    {
        return objectToUse.GetComponent<Text>();
    }
}