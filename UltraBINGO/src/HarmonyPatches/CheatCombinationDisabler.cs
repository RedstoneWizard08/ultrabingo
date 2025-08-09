using HarmonyLib;
using UnityEngine;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(CheatsController), "ProcessInput")]
public class CheatCombinationDisabler {
    [HarmonyPostfix]
    public static void DisableCheatingAttempts(GameObject consentScreen, int sequenceIndex) {
        if (!GameManager.IsInBingoLevel || Main.IsDevelopmentBuild || GameManager.TriedToActivateCheats) return;
        if (!consentScreen.activeSelf) return;
        
        MonoSingleton<OptionsManager>.Instance.UnPause();
        consentScreen.SetActive(false);

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

        Object.Instantiate(playerShotgun.explosion, playerShotgun.transform.position,
            playerShotgun.transform.rotation);

        GameManager.HumiliateSelf().Wait();
        GameManager.TriedToActivateCheats = true;
    }
}