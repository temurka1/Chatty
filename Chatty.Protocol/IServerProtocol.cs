using Chatty.Protocol.Data;

namespace Chatty.Protocol
{
    public interface IServerProtocol
    {
        /// <summary>
        /// Обрабатывает пакет, пришедший на сервер от клиента
        /// </summary>
        void ProcessPacketFromClient(Packet messagePacket);
    }
}
