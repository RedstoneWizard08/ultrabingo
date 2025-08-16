using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UltraBINGO.API;
using UltraBINGO.Net;
using UltraBINGO.UI;
using UltraBINGO.Util;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class ModVerificationResponse : IncomingPacket {
    [JsonProperty] public required List<string> NonWhitelistedMods;
    [JsonProperty] public required string LatestVersion;
    [JsonProperty] public required string Motd;
    [JsonProperty] public required string AvailableRanks;

    public override void Handle() {
        GameManager.ModListCheckPassed = NonWhitelistedMods.Count == 0;

        if (!GameManager.ModListCheckPassed) UIManager.nonWhitelistedMods = NonWhitelistedMods;

        var localVersion = new Version(Main.PluginVersion);
        var latestVersion = new Version(LatestVersion);
        var versionText = CommonFunctions.GetGameObjectChild(BingoMainMenu.VersionInfo, "VersionNum")?.GetComponent<TextMeshProUGUI>();

        if (versionText is not null)
            versionText.text = Main.PluginVersion;

        switch (localVersion.CompareTo(latestVersion)) {
            case -1: {
                Logging.Message("--UPDATE AVAILABLE--");
                Main.UpdateAvailable = true;
                CommonFunctions.GetGameObjectChild(BingoMainMenu.VersionInfo, "UpdateText")?.SetActive(true);
                break;
            }
            default: {
                Main.UpdateAvailable = false;
                break;
            }
        }

        var motdText = CommonFunctions.GetGameObjectChild(BingoMainMenu.motdContainer, "Content")?.GetComponent<TextMeshProUGUI>();

        if (motdText is not null) {
            motdText.text = Motd;
        }

        BingoMainMenu.motd = Motd;

        if (AvailableRanks != "") {
            var rankSelector = CommonFunctions.GetGameObjectChild(BingoMainMenu.RankSelection, "Dropdown")
                ?.GetComponent<TMP_Dropdown>();

            rankSelector?.ClearOptions();

            var ranks = AvailableRanks.Split(',').ToList();

            rankSelector?.AddOptions(ranks);

            BingoMainMenu.ranks = ranks;
            GameManager.RequestedRank = rankSelector?.options[0].text;

            //Check if the previously used rank is available in the list. If so, set it as default.
            if (ranks.Contains(Main.ModConfig.LastRankUsed.Value ?? string.Empty)) {
                // Why does this empty block exist?
            }

            if (rankSelector is not null) {
                //NetworkManager.requestedRank = NetworkManager.lastRankUsedConfig.Value;
                rankSelector.value = ranks.IndexOf(Main.ModConfig.LastRankUsed.Value ?? string.Empty);
            }

            GameManager.hasRankAccess = true;
        } else {
            BingoMainMenu.RankSelection?.SetActive(false);
        }

        GameManager.ModListCheckDone = true;
        Main.NetworkManager.Socket.Disconnect(1000, "ModCheckDone");
    }
}