using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("CreateRoom")]
public class CreateRoomRequest : BasePacket {
    public required string RoomName;
    public required string RoomPassword;
    public required int MaxPlayers;
    public required bool PRankRequired;
    public required string HostSteamName;
    public required string HostSteamId;
    public required string Rank;
}
