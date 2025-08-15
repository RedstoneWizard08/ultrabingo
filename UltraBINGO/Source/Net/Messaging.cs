using System;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.Util;

namespace UltraBINGO.Net;

public static class Messaging {
    /// <summary>
    /// Decode base64 messages received from the server.
    /// </summary>
    private static string DecodeMessage(string encodedMessage) =>
        System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedMessage));


    /// <summary>
    /// Encode base64 messages to send to the server.
    /// </summary>
    private static string EncodeMessage(string message) =>
        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message));

    /// <summary>
    /// Decode raw packet data from a string.
    /// </summary>
    public static EncapsulatedMessage? DecodeRawPacket(string data) =>
        JsonConvert.DeserializeObject<EncapsulatedMessage>(DecodeMessage(data));

    /// <summary>
    /// Encode a packet to a string.
    /// </summary>
    public static string? EncodePacket(BasePacket packet) {
        if (!PacketManager.Packets.ContainsKey(packet.GetType())) {
            Logging.Error($"Unregistered packet type: {packet.GetType()}. Failed to send!");
            return null;
        }

        var packetInfo = PacketManager.Packets[packet.GetType()];

        return EncodeMessage(
            JsonConvert.SerializeObject(
                new EncapsulatedMessage {
                    Contents = packetInfo.Serialize(packet),
                    Header = packetInfo.Name
                }
            )
        );
    }
}