using Chatty.Protocol.Data;

namespace Chatty.Protocol
{
    public interface IClientProtocol
    {
        /// <summary>
        /// 
        /// </summary>
        void ProcessPacketFromServer(Packet packet);
    }
}
