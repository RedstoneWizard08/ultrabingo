using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(CheatsController), "Start")]
public class RemoveCheatFlagInBingo {
    [HarmonyPostfix]
    public static void disableCheatFlagInBingo() {
        if (GameManager.IsInBingoLevel) MonoSingleton<AssistController>.Instance.cheatsEnabled = false;
    }
}

[HarmonyPatch(typeof(CheatsController), "ActivateCheats")]
public class CheatDisabler {
    [HarmonyPrefix]
    public static bool disableCheatFunctions() {
        if (GameManager.IsInBingoLevel) {
            var cheatManager = GetGameObjectChild(GetInactiveRootObject("Canvas"), "Cheat Menu");
            cheatManager.GetComponentInChildren<SandboxHud>().enabled = false;
            cheatManager.GetComponentInChildren<CheatsManager>().enabled = false;
            cheatManager.SetActive(false);
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(CheatsManager), "Start")]
public class CheatDisablerAux {
    [HarmonyPrefix]
    public static bool disableCheatFunctions() {
        if (GameManager.IsInBingoLevel) {
            var cheatManager = MonoSingleton<CheatsManager>.Instance.gameObject;
            cheatManager.GetComponentInChildren<SandboxHud>().enabled = false;
            cheatManager.GetComponentInChildren<CheatsManager>().enabled = false;
            cheatManager.SetActive(false);
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(CheatsController), "ProcessInput")]
public class CheatCombinationDisabler {
    [HarmonyPostfix]
    public static void disableCheatingAttempts(GameObject ___consentScreen, int ___sequenceIndex) {
        if (GameManager.IsInBingoLevel && !Main.IsDevelopmentBuild && !GameManager.TriedToActivateCheats)
            if (___consentScreen.activeSelf) {
                MonoSingleton<OptionsManager>.Instance.UnPause();
                ___consentScreen.SetActive(false);

                var deathScreen = GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Canvas"), "BlackScreen"),
                    "YouDiedText");
                //Need to disable the TextOverride component.
                var test = deathScreen.GetComponents(typeof(Component));
                var bhvr = (Behaviour)test[3];
                bhvr.enabled = false;

                var youDiedText = GetTextFromGameObject(deathScreen);
                youDiedText.text = "NUH UH" + "\n\n\n\n\n" + "NOW DON'T DO IT AGAIN";

                var playerShotgun = GameObject.Find("Player/Main Camera/Guns/Shotgun Pump(Clone)")
                    .GetComponentInChildren<Shotgun>();
                var playerLogic = GameObject.Find("Player").GetComponentInChildren<NewMovement>();
                playerLogic.hp = 1;

                Object.Instantiate<GameObject>(playerShotgun.explosion, playerShotgun.transform.position,
                    playerShotgun.transform.rotation);

                GameManager.HumiliateSelf();
                GameManager.TriedToActivateCheats = true;
            }
    }
}