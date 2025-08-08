using System.Collections.Generic;

namespace UltraBINGO.NetworkMessages;

public class UpdateMapPool : SendMessage {
    public string messageType = "UpdateMapPool";
    public int gameId;
    public List<string> mapPoolIds;

    public RegisterTicket ticket;
}