using System.Collections.Generic;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet("VerifyModList")]
public class VerifyModRequest : BasePacket {
    public required List<string> ClientModList;
    public required string SteamId;
}