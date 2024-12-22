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
}

public static class ModVerificationHandler
{
    public static void handle(ModVerificationResponse response)
    {
        NetworkManager.modlistCheck = response.status;
        NetworkManager.DisconnectWebSocket(1000,"ModCheckDone");
    }
}