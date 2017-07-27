using System.Net;

namespace Chatty.Protocol
{
    public interface IClient
    {
        /// <summary>
        /// Запускает клиент
        /// </summary>
        bool Start(IClientProtocol clientProtocol, string name, IPAddress ipAdress, int port);

        /// <summary>
        /// Отключение клиента от сервера, иниициированное клиентом
        /// </summary>
        void DisconnectFromServer();

        /// <summary>
        /// Отключение клиента от сервера, инициированное сервером
        /// </summary>
        void DisconnectedByServer();

        /// <summary>
        /// Устанавливает уникальный идентификатор клиенту
        /// </summary>
        void SetUid(string uid);

        /// <summary>
        /// Получение сообщения от другого клиента
        /// </summary>
        void ReceiveMessageFromClient(string senderID, string message);

        /// <summary>
        /// Отправка сообщения другому клиенту
        /// </summary>
        void SendMessageToClient(string receiverId, string messageText);
    }
}
