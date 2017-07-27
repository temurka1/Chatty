using Chatty.Protocol;
using Chatty.Protocol.Data;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Chatty.Client
{
    public class ChattyClient: IClient
    {
        private string _uid;
        private string _name;

        private bool   _isConnected;

        private Socket _socket;
        private Thread _thread;

        private IClientProtocol _clientProtocol;

        /// <inheritdoc />
        public bool Start(IClientProtocol clientProtocol, string name, IPAddress ipAdress, int port = 4242)
        {
            _name = name;
            _clientProtocol = clientProtocol;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAdress, port);

            try
            {
                _socket.Connect(ipEndPoint);

                _isConnected = true;

                _thread = new Thread(OnReceiveMessageFromServer);
                _thread.Start();
            }
            catch (SocketException)
            {
                _isConnected = false;
            }

            return _isConnected;
        }

        /// <inheritdoc />
        public void SetUid(string uid)
        {
            _uid = uid;

            Packet messagePacket = new Packet(PacketType.Register, _uid, new List<string> { _name });
            _socket.Send(messagePacket.ToBytes());
        }

        /// <inheritdoc />
        public void DisconnectedByServer()
        {
            if (_isConnected)
            {
                _isConnected = false;
                _socket.Close();
            }
        }

        /// <inheritdoc />
        public void DisconnectFromServer()
        {
            if (_isConnected)
            {
                Packet messagePacket = new Packet(PacketType.Disconnect, _uid, new List<string> { _name });
                _socket.Send(messagePacket.ToBytes());

                _isConnected = false;
                _thread.Abort();
            }
        }

        /// <inheritdoc />
        public void ReceiveMessageFromClient(string senderID, string message)
        {
        }

        /// <summary>
        /// Отправляет текстовое сообщение другому пользователю
        /// </summary>
        public void SendMessageToClient(string receiverId, string messageText)
        {
            Packet messagePacket = new Packet(PacketType.ChatWithSomeone, _uid, new List<string>(2) { receiverId, messageText });
            _socket.Send(messagePacket.ToBytes());
        }

        /// <summary>
        /// Преобразует переданный этому клиенту байты в пакеты и обрабатывает их, используя протокол клиента
        /// </summary>
        private void OnReceiveMessageFromServer()
        {
            while (_isConnected)
            {
                try
                {
                    byte[] buffer = new byte[_socket.SendBufferSize];
                    int readBytes = _socket.Receive(buffer);

                    if (readBytes > 0)
                        _clientProtocol.ProcessPacketFromServer(new Packet(buffer));
                }
                catch (SocketException)
                {
                    _socket.Close();
                    _thread.Abort();
                }
            }
        }
    }
}
