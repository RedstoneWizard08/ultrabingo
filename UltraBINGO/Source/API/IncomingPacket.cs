using System.Threading.Tasks;

namespace UltraBINGO.API;

public abstract class IncomingPacket : BasePacket {
    public abstract Task Handle();
}