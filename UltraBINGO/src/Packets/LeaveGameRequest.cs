using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("LeaveGame")]
public class LeaveGameRequest {
    public required int RoomId;
    public required string Username;
    public required string SteamId;
}