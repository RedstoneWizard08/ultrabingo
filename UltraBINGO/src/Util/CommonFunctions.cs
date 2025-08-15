using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UltraBINGO.Util;

public static class CommonFunctions {
    public static string SanitiseUsername(string rawUsername) => Regex.Replace(rawUsername, @"\p{Cs}", "");

    private static bool CheckIfLevelSaveExists(string savePath, string fileName) => File.Exists(
        Path.Combine(savePath, string.Format("Slot{0}/" + fileName, GameProgressSaver.currentSlot + 1))
    );

    public static bool HasUnlockedMod() =>
        CheckIfLevelSaveExists(
            Path.Combine(
                (SystemInfo.deviceType == DeviceType.Desktop
                    ? Directory.GetParent(Application.dataPath)?.FullName
                    : Application.persistentDataPath) ?? throw new InvalidOperationException(),
                "Saves"
            ),
            "lvl29progress.bepis"
        );

    public static string GetSceneName() => SceneHelper.CurrentScene;

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

    public static GameObject? FindObject(GameObject? parent, params string[] path) =>
        path.Aggregate(parent, GetGameObjectChild);

    public static GameObject? FindObjectWithInactiveRoot(params string[] path) {
        var actual = new Queue<string>(path);

        return actual.Aggregate(GetInactiveRootObject(actual.Dequeue()), GetGameObjectChild);
    }

    public static TextMeshProUGUI GetTextMeshProGUI(GameObject objectToUse) =>
        objectToUse.GetComponent<TextMeshProUGUI>();

    public static Text GetTextFromGameObject(GameObject objectToUse) => objectToUse.GetComponent<Text>();

    public static string GetFormattedTime(float time) {
        var secs = time;

        float mins = 0;

        while (secs >= 60f) {
            secs -= 60f;
            mins += 1f;
        }

        return $"{mins}:{secs:00.000}";
    }
}