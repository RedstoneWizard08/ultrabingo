using HarmonyLib;
using TMPro;
using UltraBINGO.Util;
using UnityEngine;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(LevelStats), "Start")]
public class StatWindowStart {
    private static GameObject? _originalChallengeText;

    [HarmonyPostfix]
    public static void SetupStatWindow(ref LevelStats instance) {
        if (!GameManager.IsInBingoLevel) return;
        
        //Prime or Encore level
        if (CommonFunctions.GetSceneName().Contains("P-") || CommonFunctions.GetSceneName().Contains("-E")) {
            var majorAssists = instance.majorAssists.transform.parent.gameObject;
            majorAssists.GetComponent<TextMeshProUGUI>().text = "TO BEAT:";

            _originalChallengeText = CommonFunctions.GetGameObjectChild(instance.gameObject, "Challenge Title");
            _originalChallengeText?.GetComponent<TextMeshProUGUI>().SetText("CLAIMED BY:");
            _originalChallengeText?.SetActive(true);

            instance.GetComponent<RectTransform>().sizeDelta = new Vector2(285f, 285f);
        }
        //Normal level
        else {
            var secrets = instance.secrets[0].transform.parent.gameObject;
            secrets.SetActive(false);

            var challenge = instance.challenge.transform.parent.gameObject;
            challenge.GetComponent<TextMeshProUGUI>().text = "TO BEAT:";

            var majorAssists = instance.majorAssists.transform.parent.gameObject;
            majorAssists.GetComponent<TextMeshProUGUI>().text = "CLAIMED BY:";
        }
    }
}