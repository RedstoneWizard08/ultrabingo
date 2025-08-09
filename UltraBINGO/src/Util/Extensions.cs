using System.Threading.Tasks;
using UltraBINGO.API;
using UltraBINGO.Packets;

namespace UltraBINGO.Util;

public static class Extensions {
    public static Task TryHandle<T>(this T? packet) where T : IncomingPacket {
        return packet?.Handle() ?? Task.CompletedTask;
    }
}