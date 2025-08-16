using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.Net;
using UltraBINGO.Types;
using UltraBINGO.UI;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class FetchGamesResponse : IncomingPacket {
    [JsonProperty] public required string Status;
    [JsonProperty] public required List<PublicGameData> GameData;

    public override void Handle() {
        Main.NetworkManager.Socket.Disconnect(1000, "GameList");
        BingoBrowser.PopulateGames(GameData);
    }
}