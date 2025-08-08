using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UltraBINGO;

public static class CommonFunctions {
    public static string SanitiseUsername(string rawUsername) {
        return Regex.Replace(rawUsername, @"\p{Cs}", "");
    }

    private static bool CheckIfLevelSaveExists(string savePath, string fileName) {
        var fullPath = Path.Combine(savePath, string.Format("Slot{0}/" + fileName, GameProgressSaver.currentSlot + 1));
        return File.Exists(fullPath);
    }

    public static bool HasUnlockedMod() {
        var savePath =
            Path.Combine(
                (SystemInfo.deviceType == DeviceType.Desktop
                    ? Directory.GetParent(Application.dataPath)?.FullName
                    : Application.persistentDataPath) ?? throw new InvalidOperationException(), "Saves");

        var _74Beat = CheckIfLevelSaveExists(savePath, "lvl29progress.bepis");

        return _74Beat;
    }

    public static string GetSceneName() {
        return SceneHelper.CurrentScene;
    }

    public static GameObject? GetInactiveRootObject(string objectName) {
        var rootList = new List<GameObject>();
        SceneManager.GetActiveScene().GetRootGameObjects(rootList);

        return rootList.FirstOrDefault(child => child.name == objectName);
    }

    public static IEnumerator WaitForSeconds(float seconds) {
        yield return new WaitForSeconds(seconds);
    }

    public static GameObject? GetGameObjectChild(GameObject? parentObject, string childToFind) {
        try {
            var childToReturn = parentObject?.transform.Find(childToFind).gameObject;
            return childToReturn;
        } catch (Exception) {
            return null;
        }
    }

    public static TextMeshProUGUI GetTextMeshProGUI(GameObject objectToUse) {
        return objectToUse.GetComponent<TextMeshProUGUI>();
    }

    public static Text GetTextFromGameObject(GameObject objectToUse) {
        return objectToUse.GetComponent<Text>();
    }

    public static string GetFormattedTime(float time) {
        var secs = time;

        float mins = 0;

        while (secs >= 60f) {
            secs -= 60f;
            mins += 1f;
        }

        return mins + ":" + secs.ToString("00.000");
    }
}