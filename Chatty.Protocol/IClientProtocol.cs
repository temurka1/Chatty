using Chatty.Protocol.Data;

namespace Chatty.Protocol
{
    public interface IClientProtocol
    {
        /// <summary>
        /// Обрабатывает пакет, пришедший от сервера этому клиенту
        /// </summary>
        void ProcessPacketFromServer(Packet packet);
    }
}
