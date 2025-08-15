using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UltraBINGO.Packets;

namespace UltraBINGO.API;

public class PacketInfo {
    public required Type Type;
    public required string Name;
    public required PacketDirection Direction;

    [UsedImplicitly]
    public string Serialize(object value) {
        if (value.GetType() != Type) throw new ArgumentException("Value type did not match packet type!");

        return JsonConvert.SerializeObject(value);
    }

    [UsedImplicitly]
    public IncomingPacket? Deserialize(string value) {
        if (Direction != PacketDirection.ServerToClient)
            throw new InvalidOperationException("Cannot deserialize a ClientToServer packet!");

        return JsonConvert.DeserializeObject(value, Type) as IncomingPacket;
    }

    [UsedImplicitly]
    public IncomingPacket? DeserializeOrNull(string value) {
        if (Direction != PacketDirection.ServerToClient) return null;

        return JsonConvert.DeserializeObject(value, Type) as IncomingPacket;
    }

    public async Task<Exception?> Handle(string value) {
        try {
            await (
                Deserialize(value)?.Handle() ??
                throw new InvalidOperationException("Cannot handle a packet that couldn't be deserialized!")
            );
        } catch (Exception e) {
            return e;
        }

        return null;
    }

    public static PacketInfo Create(Type type, Packet attr) {
        return new PacketInfo {
            Type = type,
            Name = attr.Name ?? type.Name,
            Direction = attr.Direction,
        };
    }
}