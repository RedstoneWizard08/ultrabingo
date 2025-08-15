using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class RegisterTicket : BasePacket {
    public required string SteamTicket;
    public required string SteamId;
    public required string SteamUsername;
    public required int GameId;
}