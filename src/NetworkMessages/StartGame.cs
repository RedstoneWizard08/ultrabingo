using System.Collections.Generic;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class StartGameRequest : SendMessage
{
    public string messageType = "StartGame";
    
    public int roomId;
    
}

public class StartGameResponse : MessageResponse
{
    public string teamColor;
    public List<string> teammates;
}