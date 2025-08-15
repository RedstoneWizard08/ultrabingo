using System;
using HarmonyLib;
using SettingsMenu.Components;
using SettingsMenu.Models;
using UltraBINGO.UI;
using UltraBINGO.Util;
using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(SettingsPageBuilder), "BuildPage")]
public static class SettingsPageBuilderPatches {
    [HarmonyPostfix]
    public static void DisableMajorAssistsOptions(ref SettingsPage settingsPage) {
        if (GameManager.IsInBingoLevel && !GameManager.CurrentGame.IsGameFinished()) {
            //Hide the PluginConfig button
            var optionsMenu = MonoSingleton<OptionsMenuToManager>.Instance.optionsMenu;

            try {
                GetGameObjectChild(
                    GetGameObjectChild(optionsMenu.gameObject, "Navigation Rail"),
                    "PluginConfiguratorButton(Clone)"
                )?.SetActive(false);
            } catch (Exception) {
                Logging.Warn("Couldn't find PluginConfigurator button in options - Is it not working?");
            }

            if (settingsPage.name != "Assist") return;
            
            var assistsList =
                GetGameObjectChild(
                    GetGameObjectChild(
                        GetGameObjectChild(GetGameObjectChild(optionsMenu.gameObject, "Pages"), "Assist"),
                        "Scroll Rect"
                    ),
                    "Contents"
                );

            GetGameObjectChild(assistsList, "-- Major Assists --")?.SetActive(false);
            GetGameObjectChild(assistsList, "Game Speed")?.SetActive(false);
            GetGameObjectChild(assistsList, "Damage Taken")?.SetActive(false);
            GetGameObjectChild(assistsList, "Boss Fight Difficulty Override")?.SetActive(false);
            GetGameObjectChild(assistsList, "Infinite Stamina")?.SetActive(false);
            GetGameObjectChild(assistsList, "Disable Whiplash Hard Damage")?.SetActive(false);
            GetGameObjectChild(assistsList, "Disable All Hard Damage")?.SetActive(false);
            GetGameObjectChild(assistsList, "Disable Weapon Freshness")?.SetActive(false);
            GetGameObjectChild(assistsList, "Disable Assist Popup")?.SetActive(false);

            var disabledNotification =
                UIHelper.CreateText(
                    "Major assists are <color=orange>disabled</color> while playing Baphomet's Bingo.",
                    26,
                    "TextDisabled"
                );
            
            disabledNotification.transform.SetParent(assistsList?.transform);
        }
    }
}