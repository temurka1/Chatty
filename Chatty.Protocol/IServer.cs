using Chatty.Protocol.Data;
using System.Collections.Generic;

namespace Chatty.Protocol
{
    public interface IServer
    {
        /// <summary>
        /// запускает сервер
        /// </summary>
        void Start(IServerProtocol serverProtocol, string ipAdress, int port);

        /// <summary>
        /// посылает пакет клиенту, указанному в пакете
        /// </summary>
        void SendMessageToClient(Packet packet);

        /// <summary>
        /// посылает пакет все активным клиентам
        /// </summary>
        void BroadcastMessageToClients(Packet packet);

        /// <summary>
        /// закрывает соединение клиента
        /// </summary>
        void CloseClientConnection(string clientUid);

        /// <summary>
        /// устанавливает имя для клиента
        /// </summary>
        void SetClientName(string clientUid, string name);

        /// <summary>
        /// возвращает список (id1, name1, id2, name2, ...) для всех клиентов
        /// </summary>
        List<string> GetActiveClientsDescriptions();
    }
}
