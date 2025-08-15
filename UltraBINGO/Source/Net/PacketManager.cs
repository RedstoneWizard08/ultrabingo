using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using UltraBINGO.API;

namespace UltraBINGO.Net;

public static class PacketManager {
    public static FrozenDictionary<Type, PacketInfo> Packets { get; private set; } =
        FrozenDictionary<Type, PacketInfo>.Empty;

    public static FrozenDictionary<string, PacketInfo> PacketsByName { get; private set; } =
        FrozenDictionary<string, PacketInfo>.Empty;

    private static readonly Dictionary<Type, PacketInfo> PacketRegistry = new();
    private static bool _wasFrozen;

    public static void Register(Type type, PacketInfo info) {
        if (_wasFrozen) throw new InvalidOperationException("PacketManager is already frozen!");

        PacketRegistry[type] = info;
    }

    public static void Freeze() {
        Packets = PacketRegistry.ToFrozenDictionary();

        PacketsByName = Packets
            .Select(entry => KeyValuePair.Create(entry.Value.Name, entry.Value))
            .ToFrozenDictionary();

        _wasFrozen = true;
    }
}