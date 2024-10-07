using System;
using System.Collections.Generic;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class CheatActivation : SendMessage
{
    public string messageType = "CheatActivation";
    
    public int gameId;
    public string username;
    public string steamId;
}

public class CheatNotification : MessageResponse
{
    public string playerToHumil;
}

public static class CheatNotificationHandler
{
    public static List<string> messages = new List<string>()
    {
        "Unfortunately, I ate them all.",
        "Their punishment is one ranked match of League.",
        "What a silly person.",
        "They may have spontaneously imploded.",
        "Their death was a canon event.",
        "Jakito does not approve.",
        "Their punishment is 1 hour of grinding 1-4.",
        "Their attempt was a futile struggle, doomed from the very start."
        
    };
    
    public static void handle(CheatNotification response)
    {
        Random random = new Random();
        string msg = response.playerToHumil + " tried to enable cheats.\n" + messages[random.Next(messages.Count)];
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
        
    }
}