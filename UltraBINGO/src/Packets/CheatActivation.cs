using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class CheatActivation : BasePacket {
    public required int GameId;
    public required string Username;
    public required string SteamId;
}
