using Chatty.Protocol.Data;

namespace Chatty.Protocol
{
    public interface IServerProtocol
    {
        void ProcessPacketFromClient(Packet messagePacket);
    }
}
