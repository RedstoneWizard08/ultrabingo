using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.NetworkMessages;

public class VerifyModRequest : SendMessage
{
    public string messageType = "VerifyModList";
    
    public List<string> clientModList;
    public string steamId;
    
    public VerifyModRequest(List<string> clientModList,string steamId)
    {
        this.clientModList = clientModList;
        this.steamId = steamId;
    }
}

public class ModVerificationResponse : MessageResponse
{
    public bool status;
    
    public string latestVersion;
    
    public string motd;
    
    public string availableRanks;
 
}

public static class ModVerificationHandler
{
    public static void handle(ModVerificationResponse response)
    {
        NetworkManager.modlistCheckPassed = response.status;
        
        Version localVersion = new Version(Main.pluginVersion);
        Version latestVersion = new Version(response.latestVersion);
        
        GetGameObjectChild(BingoMainMenu.VersionInfo,"VersionNum").GetComponent<TextMeshProUGUI>().text = Main.pluginVersion;
        
        switch(localVersion.CompareTo(latestVersion))
        {
            case -1:
            {
                Logging.Message("--UPDATE AVAILABLE--");
                Main.UpdateAvailable = true;
                GetGameObjectChild(BingoMainMenu.VersionInfo,"UpdateText").SetActive(true);
                break;
            }
            default: {Main.UpdateAvailable = false;break;}
        }
        
        GetGameObjectChild(BingoMainMenu.MOTDContainer,"Content").GetComponent<TextMeshProUGUI>().text = response.motd;
        BingoMainMenu.MOTD = response.motd;
        
        if(response.availableRanks != "")
        {
            TMP_Dropdown rankSelector = GetGameObjectChild(BingoMainMenu.RankSelection,"Dropdown").GetComponent<TMP_Dropdown>();
            rankSelector.ClearOptions();
            List<string> ranks = response.availableRanks.Split(',').ToList();
            rankSelector.AddOptions(ranks);
            BingoMainMenu.ranks = ranks;
            NetworkManager.requestedRank = rankSelector.options[0].text;
            
            GameManager.hasRankAccess = true;
        }
        else
        {
            BingoMainMenu.RankSelection.SetActive(false);
        }
                    
        NetworkManager.modlistCheckDone = true;
        NetworkManager.DisconnectWebSocket(1000,"ModCheckDone");
    }
}