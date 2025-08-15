using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class RegisterTicket : BasePacket {
    [JsonProperty] public required string SteamTicket;
    [JsonProperty] public required string SteamId;
    [JsonProperty] public required string SteamUsername;
    [JsonProperty] public required int GameId;
    
    public static RegisterTicket Create() =>
        new() {
            SteamId = Steamworks.SteamClient.SteamId.ToString(),
            SteamTicket = SteamManager.GetSteamTicket(),
            SteamUsername = Steamworks.SteamClient.Name,
            GameId = GameManager.CurrentGame.GameId
        };
}