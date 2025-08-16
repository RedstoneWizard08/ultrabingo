using HarmonyLib;
using UltraBINGO.Util;
using UnityEngine;
using static UltraBINGO.Util.CommonFunctions;

// ReSharper disable InconsistentNaming

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch]
public static class CheatsControllerPatches {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CheatsController), nameof(CheatsController.ActivateCheats))]
    public static bool DisableCheatFunctions() {
        if (!GameManager.IsInBingoLevel) return true;

        var cheatManager = FindObjectWithInactiveRoot("Canvas", "Cheat Menu");

        if (cheatManager == null) return false;

        cheatManager.GetComponentInChildren<SandboxHud>().enabled = false;
        cheatManager.GetComponentInChildren<CheatsManager>().enabled = false;
        cheatManager.SetActive(false);

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CheatsController), nameof(CheatsController.ProcessInput))]
    public static void DisableCheatingAttempts(GameObject ___consentScreen, int ___sequenceIndex) {
        if (!GameManager.IsInBingoLevel || Main.IsDevelopmentBuild || GameManager.TriedToActivateCheats) return;
        if (!___consentScreen.activeSelf) return;

        MonoSingleton<OptionsManager>.Instance.UnPause();
        ___consentScreen.SetActive(false);

        var deathScreen = FindObjectWithInactiveRoot("Canvas", "BlackScreen", "YouDiedText");

        //Need to disable the TextOverride component.
        var test = deathScreen?.GetComponents(typeof(Component));
        var behaviour = test?[3] as Behaviour;

        if (behaviour != null) behaviour.enabled = false;

        if (deathScreen != null) {
            var youDiedText = GetTextFromGameObject(deathScreen);

            youDiedText.text = "NUH UH" + "\n\n\n\n\n" + "NOW DON'T DO IT AGAIN";
        }

        var playerShotgun = GameObject.Find("Player/Main Camera/Guns/Shotgun Pump(Clone)")
            .GetComponentInChildren<Shotgun>();

        var playerLogic = GameObject.Find("Player").GetComponentInChildren<NewMovement>();

        playerLogic.hp = 1;

        Object.Instantiate(
            playerShotgun.explosion,
            playerShotgun.transform.position,
            playerShotgun.transform.rotation
        );

        GameManager.HumiliateSelf();
        GameManager.TriedToActivateCheats = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CheatsController), nameof(CheatsController.Start))]
    public static void DisableCheatFlagInBingo() {
        if (GameManager.IsInBingoLevel) MonoSingleton<AssistController>.Instance.cheatsEnabled = false;
    }
}