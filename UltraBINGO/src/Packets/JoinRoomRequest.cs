using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("JoinRoom")]
public class JoinRoomRequest {
    public required string Password;
    public required string Username;
    public required string SteamId;
    public required string Rank;
}