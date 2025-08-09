using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("UpdateRoomSettings")]
public class UpdateRoomSettingsRequest {
    public required int RoomId;
    public required int MaxPlayers;
    public required int MaxTeams;
    public required int TimeLimit;
    public required int TeamComposition;
    public required int GameMode;
    public required bool PRankRequired;
    public required int Difficulty;
    public required int GridSize;
    public required bool DisableCampaignAltExits;
    public required int GameVisibility;
    public required RegisterTicket Ticket;
}