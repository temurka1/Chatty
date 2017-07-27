using Chatty.Protocol.Data;
using System;
using System.Collections.Generic;

namespace Chatty.Protocol.Server
{
    /// <summary>
    /// 
    /// </summary>
    public class ChattyServerProtocol : IServerProtocol
    {
        private readonly Action<IServer, Packet> _OnChatMessageReceived = (server, pack) =>
        {
            Logger.Instance.Info($"Message from {pack.senderID} to { pack.data[0] }: {pack.data[1]}");
            server.SendMessageToClient(pack);
        };

        private readonly Action<IServer, Packet> _OnDisconnectReceived  = (server, pack) =>
        {
            Logger.Instance.Info($"Client disconnected: name = {pack.data[0]}, id = { pack.senderID }");

            List<string> clients = server.GetActiveClientsDescriptions();
            clients.Remove(pack.data[0]);
            clients.Remove(pack.senderID);

            server.BroadcastMessageToClients(new Packet(PacketType.ActiveClients, "server", clients));
            server.CloseClientConnection(pack.senderID);
        };

        private readonly Action<IServer, Packet> _OnRegisterReceived = (server, pack) =>
        {
            Logger.Instance.Info($"Client connected: name = { pack.data[0] }, id = { pack.senderID }");

            server.SetClientName(pack.senderID, pack.data[0]);
            server.BroadcastMessageToClients(new Packet(PacketType.ActiveClients, "server", server.GetActiveClientsDescriptions()));
        };

        private readonly IServer _serverInstance;

        /// <summary>
        /// Конструктор
        /// </summary>
        public ChattyServerProtocol(IServer _serverInstance)
        {
            this._serverInstance = _serverInstance;
        }

        /// <inheritdoc />
        public void ProcessPacketFromClient(Packet messagePacket)
        {
            // Пакеты:
            // - Register (data[0] - айди этого клиента)
            // - ChatWithSomeone (senderId - от кого пришло сообщение, data[0] - кому отправлялось сообщение, data[1] - текст сообщения)
            // - Disconnect

            switch (messagePacket.type)
            {
                case PacketType.Register:
                    _OnRegisterReceived(_serverInstance, messagePacket);
                    break;

                case PacketType.ChatWithSomeone:
                    _OnChatMessageReceived(_serverInstance, messagePacket);
                    break;

                case PacketType.Disconnect:
                    _OnDisconnectReceived(_serverInstance, messagePacket);
                    break;
            }
        }
    }
}
