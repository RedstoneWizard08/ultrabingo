using System;
using JetBrains.Annotations;

namespace UltraBINGO.API;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class Packet(string? name = null, PacketDirection direction = PacketDirection.ClientToServer) : Attribute {
    /**
     * The name of the packet. Defaults to the class name.
     */
    public readonly string? Name = name;

    /**
     * The direction the packet is going. Defaults to `ClientToServer`.
     */
    public readonly PacketDirection Direction = direction;

    public Packet(PacketDirection direction) : this(null, direction) {
    }
}