using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class RegisterTicket {
    public required string SteamTicket;
    public required string SteamId;
    public required string SteamUsername;
    public required int GameId;
}