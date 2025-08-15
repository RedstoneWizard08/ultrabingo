using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("SubmitRun")]
public class SubmitRunRequest : BasePacket {
    public required string Team;
    public required int GameId;
    public required int Column;
    public required int Row;
    public required string LevelName;
    public required string LevelId;
    public required string PlayerName;
    public required string SteamId;
    public required float Time;
    public required RegisterTicket Ticket;
}