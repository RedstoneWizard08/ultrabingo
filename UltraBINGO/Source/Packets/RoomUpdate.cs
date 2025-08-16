using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.UI;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class RoomUpdate : IncomingPacket {
    [JsonProperty] public required int MaxPlayers;
    [JsonProperty] public required int MaxTeams;
    [JsonProperty] public required int TimeLimit;
    [JsonProperty] public required int TeamComposition;
    [JsonProperty] public required int GameMode;
    [JsonProperty] public required bool PRankRequired;
    [JsonProperty] public required int Difficulty;
    [JsonProperty] public required int GridSize;
    [JsonProperty] public required bool DisableCampaignAltExits;
    [JsonProperty] public required int GameVisibility;
    [JsonProperty] public required bool WereTeamsReset;

    public override void Handle() {
        BingoLobby.UpdateFromNotification(this);
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("The host has updated the room settings.");
    }
}