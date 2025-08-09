using System.Threading.Tasks;

namespace UltraBINGO.API;

public abstract class IncomingPacket {
    public abstract Task Handle();
}