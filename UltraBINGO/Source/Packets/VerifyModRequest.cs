using System.Collections.Generic;
using Newtonsoft.Json;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("VerifyModList")]
public class VerifyModRequest : BasePacket {
    [JsonProperty] public required List<string> ClientModList;
    [JsonProperty] public required string SteamId;
}