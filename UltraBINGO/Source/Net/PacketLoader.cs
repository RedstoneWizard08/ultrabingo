using System;
using System.Linq;
using System.Reflection;
using UltraBINGO.API;
using UltraBINGO.Packets;
using UltraBINGO.Util;

namespace UltraBINGO.Net;

public static class PacketLoader {
    public static void Load() {
        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())) {
            TryLoadPacket(type);
        }
    }

    private static void TryLoadPacket(Type type) {
        if (PacketManager.Packets.ContainsKey(type)) return;

        var attr = type.GetCustomAttribute<Packet>(false);

        if (attr == null) return;

        var info = PacketInfo.Create(type, attr);

        Logging.Info($"Found packet {info.Name} in {type.FullName}");

        if (info.Direction == PacketDirection.ServerToClient && !type.IsSubclassOf(typeof(IncomingPacket))) {
            throw new ArgumentException(
                $"Packet in {type.FullName} defines itself as a ServerToClient packet, yet it does not inherit from IncomingPacket!"
            );
        }

        PacketManager.Register(type, info);
    }
}