using HarmonyLib;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

    [HarmonyPatch(typeof(CheatsController), "ActivateCheats")]
    public class CheatDisabler
    {
        [HarmonyPrefix]
        public static bool disableCheatFunctions()
        {
            if (GameManager.isInBingoLevel)
            {
                GameObject cheatManager = GetGameObjectChild(GetInactiveRootObject("Canvas"),"Cheat Menu");
                cheatManager.GetComponentInChildren<SandboxHud>().enabled = false;
                cheatManager.GetComponentInChildren<CheatsManager>().enabled = false;
                cheatManager.SetActive(false);
                return false;
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(CheatsManager), "Start")]
    public class CheatDisablerAux
    {
        [HarmonyPrefix]
        public static bool disableCheatFunctions()
        {
            if (GameManager.isInBingoLevel)
            {
                GameObject cheatManager = GetGameObjectChild(GetInactiveRootObject("Canvas"),"Cheat Menu");
                cheatManager.GetComponentInChildren<SandboxHud>().enabled = false;
                cheatManager.GetComponentInChildren<CheatsManager>().enabled = false;
                cheatManager.SetActive(false);
                return false;
            }
            return true;
        }
    }
    
    
    [HarmonyPatch(typeof(CheatsController), "ProcessInput")]
    public class CheatCombinationDisabler
    {
        [HarmonyPostfix]
        public static void disableCheatingAttempts(GameObject ___consentScreen, int ___sequenceIndex)
        {
            if(GameManager.isInBingoLevel && !Main.IsDevelopmentBuild)
            {
                if(___consentScreen.activeSelf)
                {
                    MonoSingleton<OptionsManager>.Instance.UnPause();
                    ___consentScreen.SetActive(false);
                    
                    GameObject deathScreen = GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Canvas"), "BlackScreen"), "YouDiedText");
                    //Need to disable the TextOverride component.
                    Component[] test = deathScreen.GetComponents(typeof(Component));
                    Behaviour bhvr = (Behaviour)test[3];
                    bhvr.enabled = false;

                    Text youDiedText = GetTextfromGameObject(deathScreen);
                    youDiedText.text = "NUH UH" + "\n\n\n\n\n" + "NOW DON'T DO IT AGAIN";
                    
                    Shotgun playerShotgun = GameObject.Find("Player/Main Camera/Guns/Shotgun Pump(Clone)").GetComponentInChildren<Shotgun>();
                    NewMovement playerLogic = GameObject.Find("Player").GetComponentInChildren<NewMovement>();
                    playerLogic.hp = 1;
                    
                    GameObject deathSplosion = UnityEngine.Object.Instantiate<GameObject>(playerShotgun.explosion, playerShotgun.transform.position,playerShotgun.transform.rotation);
                }
                
                GameManager.HumiliateSelf();
                
            }
        }
    }