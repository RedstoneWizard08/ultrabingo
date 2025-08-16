using System;
using System.Collections.Generic;
using UltraBINGO.API;

namespace UltraBINGO.Net;

public static class PacketManager {
    public static readonly Dictionary<Type, PacketInfo> Packets = [];
    public static readonly Dictionary<string, PacketInfo> PacketsByName = [];

    public static void Register(Type type, PacketInfo info) {
        Packets[type] = info;
        PacketsByName[info.Name] = info;
    }
}