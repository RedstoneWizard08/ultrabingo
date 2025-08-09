using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class RerollRequest {
    public required RegisterTicket SteamTicket;
    public required string SteamId;
    public required int GameId;
    public required int Row;
    public required int Column;
}