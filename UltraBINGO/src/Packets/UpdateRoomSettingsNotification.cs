using System.Threading.Tasks;
using UltraBINGO.API;
using UltraBINGO.UI;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class UpdateRoomSettingsNotification : IncomingPacket {
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
    public required bool WereTeamsReset;

    public override Task Handle() {
        BingoLobby.UpdateFromNotification(this);
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("The host has updated the room settings.");

        return Task.CompletedTask;
    }
}