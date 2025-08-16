using System.Collections.Generic;
using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet]
public class VerifyModList : BasePacket {
    [JsonProperty] public required List<string> ClientModList;
    [JsonProperty] public required string SteamId;
}