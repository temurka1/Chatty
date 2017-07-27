using Chatty.Protocol.Data;
using System;
using System.Collections.Generic;

namespace Chatty.Protocol.Client
{
    public class ChattyClientProtocol : IClientProtocol
    {
        private readonly Action<IClient, Packet> _OnRegisterReceived    = (client, pack) => client.SetUid(pack.data[0]);
        private readonly Action<IClient, Packet> _OnChatMessageReceived = (client, pack) => client.ReceiveMessageFromClient(pack.senderID, pack.data[1]);
        private readonly Action<IClient, Packet> _OnDisconnectReceived  = (client, pack) => client.DisconnectedByServer();

        public Action<string, string> uOnChatMessageReceived   { get; set; }
        public Action                 uOnRegisterReceivedUser  { get; set; }
        public Action                 uOnDisconnectReceived    { get; set; }

        public Action<List<string>>   uOnActiveClientsReceived { get; set; }

        private readonly IClient _clientInstance;

        /// <summary>
        /// Конструктор
        /// </summary>
        public ChattyClientProtocol(IClient _clientInstance)
        {
            this._clientInstance = _clientInstance;
        }

        /// <inheritdoc />
        public void ProcessPacketFromServer(Packet packet)
        {
            // функция обрабатывает пакеты, приходящие с сервера для данного клиента, 
            // вызывая соответствующие _On* делегаты (которые обрабатывают сами пакеты),
            // и соответствующие uOn делегаты, которые выполняют какие-либо действия, которые клиент захотел совершить при приходе конкретного пакета

            // Пакеты:
            // - Register (data[0] - айди этого клиента)
            // - ChatWithSomeone (senderId - от кого пришло сообщение, data[0] - кому отправлялось сообщение, data[1] - текст сообщения)
            // - Disconnect
            // - Registerd (data[0] - registeredId, data[1] - registeredName)
            // - Disconnected (data[0] - dcId, data[1] - dcName)

            switch (packet.type)
            {
                case PacketType.Register:
                    _OnRegisterReceived(_clientInstance, packet);
                    uOnRegisterReceivedUser();
                    break;
                    
                case PacketType.ChatWithSomeone:
                    _OnChatMessageReceived(_clientInstance, packet);
                    uOnChatMessageReceived(packet.senderID, packet.data[1]);
                    break;
                    
                case PacketType.Disconnect:
                    _OnDisconnectReceived(_clientInstance, packet);
                    uOnDisconnectReceived();
                    break;

                case PacketType.ActiveClients:
                    uOnActiveClientsReceived(packet.data);
                    break;
            }
        }
    }
}
