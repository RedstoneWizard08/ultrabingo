using System.Collections.Generic;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class UpdateMapPool : SendMessage
{
    public string messageType = "UpdateMapPool";
    public int gameId;
    public List<string> mapPoolIds;
    
    public RegisterTicket ticket;
}