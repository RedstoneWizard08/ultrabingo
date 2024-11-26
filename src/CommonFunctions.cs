using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UltraBINGO;

public static class CommonFunctions
{
    public static string sanitiseUsername(string rawUsername)
    {
        return Regex.Replace(rawUsername,@"\p{Cs}", "");
    }
    
    public static bool checkIfLevelSaveExists(string savePath, string fileName)
    {
        string fullPath = Path.Combine(savePath, string.Format("Slot{0}/" + fileName, GameProgressSaver.currentSlot + 1));
        Logging.Message(fullPath);
        return File.Exists(fullPath);
    }
    
    public static bool hasUnlockedMod()
    {
        string savePath = Path.Combine((SystemInfo.deviceType == DeviceType.Desktop) ? Directory.GetParent(Application.dataPath).FullName : Application.persistentDataPath, "Saves");
            
        bool _74Beat = checkIfLevelSaveExists(savePath,"lvl29progress.bepis");

        return _74Beat;
    }
    
    public static string getSceneName()
    {
        return SceneHelper.CurrentScene;
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

    public static TextMeshProUGUI GetTextMeshProGUI(GameObject objectToUse)
    {
        return objectToUse.GetComponent<TextMeshProUGUI>();
    }
    
    public static Text GetTextfromGameObject(GameObject objectToUse)
    {
        return objectToUse.GetComponent<Text>();
    }
}