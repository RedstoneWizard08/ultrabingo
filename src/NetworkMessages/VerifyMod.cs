using System;
using System.Collections.Generic;
using UltrakillBingoClient;

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
}

public static class ModVerificationHandler
{
    public static void handle(ModVerificationResponse response)
    {
        NetworkManager.modlistCheck = response.status;
        
        Version localVersion = new Version(Main.pluginVersion);
        Version latestVersion = new Version(response.latestVersion);
        
        switch(localVersion.CompareTo(latestVersion))
        {
            case -1: { Logging.Message("UPDATE AVAILABLE!");Main.UpdateAvailable = true; break;}
            default: { Logging.Message("No newer version detected. Assuming current version is up to date."); Main.UpdateAvailable = false;break;}
        }
                    
        
        NetworkManager.DisconnectWebSocket(1000,"ModCheckDone");
    }
}