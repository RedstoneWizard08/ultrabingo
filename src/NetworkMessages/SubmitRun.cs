using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

// Submit a run to the server.
public class SubmitRunRequest : SendMessage
{
    public new string messageType = "SubmitRun";
    
    public string team;
    public int gameId;
    
    public int column;
    public int row;
    
    public string mapName;
    public string playerName;
    public string steamId;
    
    public float time;
    public float style;
}

// Notify all players that a map has been claimed/reclaimed/improved.
public class LevelClaimNotification : PlayerNotification
{
    public int claimType; //0:Claimed , 1: Improved, 2: Reclaimed
    public string username;

    public string levelname;
    public string team;
    
    public int row;
    public int column;
    
    public float newTimeRequirement;
    public float newStyleRequirement;
    
}

public static class LevelClaimHandler
{
    public static void handle(LevelClaimNotification response)
    {
        string actionType = "";
        switch(response.claimType)
        {
            case 0: {actionType = "claimed "; break;}
            case 1: {actionType = "improved "; break;}
            case 2: {actionType = "reclaimed "; break;}
        }
        
        string broadcastString = response.username + " has <color=orange>" + actionType + response.levelname + "</color> for the " + response.team + " team.";
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(broadcastString);
        GameManager.UpdateCards(response.row,response.column,response.team,response.username,(GameManager.CurrentGame.gameSettings.gameType == 0 ? response.newTimeRequirement : response.newStyleRequirement));
    }
}