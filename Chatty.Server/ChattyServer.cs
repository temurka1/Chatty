using Chatty.Protocol;
using Chatty.Protocol.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Chatty.Server
{
    public class ClientData
    {
        public string name;
        public readonly string uid;

        public DateTime lastActive;

        public readonly Socket clientSocket;
        public Thread clientThread;

        public ClientData(Guid uid, Socket clientSocket)
        {
            this.clientSocket = clientSocket;
            this.uid = uid.ToString();
        }
    }

    public class ChattyServer: IServer
    {
        /// <summary>
        /// Сокет, на котором сервер ожидает подключения клиентов
        /// </summary>
        private Socket _listenerSocket;

        /// <summary>
        /// Список активных клиентов
        /// </summary>
        private List<ClientData> ActiveClients  
        {
            get
            {
                lock (lockObj)
                {
                    return activeClients;
                }
            }
        }

        /// <summary>
        /// Протокол работы сервера
        /// </summary>
        private IServerProtocol _serverProtocol;

        private readonly object lockObj = new object();
        private readonly List<ClientData> activeClients = new List<ClientData>();

        /// <summary>
        /// таймаут на подключение клиента, в секундах
        /// </summary>
        private const int _timeoutValue = 120;

        /// <inheritdoc />
        public void Start(IServerProtocol serverProtocol, string ipAdress, int port = 4242)
        {
            Logger.Instance.Info("Starting server...");

            _serverProtocol = serverProtocol;

            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(ipAdress), port);

            _listenerSocket.Bind(ip);

            Thread listenThread = new Thread(() =>
            {
                Timer clientsActivityCheckerTimer = new Timer(state =>
                {
                    List<string> timedoutClientsIds = ActiveClients.Where(t => DateTime.Now - t.lastActive > new TimeSpan(0, 0, 0, _timeoutValue)).Select(t => t.uid).ToList();
                    foreach (string uid in timedoutClientsIds)
                    {
                        SendMessageToClient(new Packet(PacketType.Disconnect, "server", new List<string>(1) { uid }));

                        ClientData timedoutClient = ActiveClients.First(cl => cl.uid == uid);

                        Logger.Instance.Info($"Client timedout and disconnected: name = {timedoutClient.name}, id = { timedoutClient.uid }");
                        ActiveClients.Remove(timedoutClient);
                    }

                    if (timedoutClientsIds.Count > 0)
                        BroadcastMessageToClients(new Packet(PacketType.ActiveClients, "server", GetActiveClientsDescriptions()));
                }, null, 0, 1000);

                while (true)
                {
                    try
                    {
                        _listenerSocket.Listen(10);

                        ClientData connectedClient = new ClientData(Guid.NewGuid(), _listenerSocket.Accept());

                        ActiveClients.Add(connectedClient);

                        connectedClient.lastActive   = DateTime.Now;
                        connectedClient.clientThread = new Thread(() => OnReceiveMessageFromClient(connectedClient));
                        connectedClient.clientThread.Start();

                        // после создания клиента отправим ему обратно пакет с его уникальным идентификатором
                        SendMessageToClient(new Packet(PacketType.Register, "server", new List<string>(1) {connectedClient.uid}));
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error($"Server error: {ex}");
                    }
                }
            });

            listenThread.Start();
            
            Logger.Instance.Info($"Success... Listening IP: { ipAdress }:{ port }");
        }

        /// <inheritdoc />
        public void SendMessageToClient(Packet packet)
        {
            ClientData client = GetClientById(packet.data[0]);

            client.lastActive = DateTime.Now;
            client.clientSocket.Send(packet.ToBytes());
        }

        /// <inheritdoc />
        public void BroadcastMessageToClients(Packet packet)
        {
            foreach (ClientData client in ActiveClients)
            {
                client.lastActive = DateTime.Now;
                client.clientSocket.Send(packet.ToBytes());
            }
        }

        /// <inheritdoc />
        public void CloseClientConnection(string clientUid)
        {
            ClientData client = GetClientById(clientUid);

            client.clientSocket.Close();
            ActiveClients.Remove(client);
            client.clientThread.Abort();
        }

        /// <inheritdoc />
        public void SetClientName(string clientUid, string name)
        {
            ClientData client = GetClientById(clientUid);
            client.name = name;

            client.lastActive = DateTime.Now;
        }

        /// <inheritdoc />
        public List<string> GetActiveClientsDescriptions()
        {
            List<string> cls = new List<string>();

            foreach (ClientData client in ActiveClients)
            {
                cls.Add(client.uid);
                cls.Add(client.name);
            }

            return cls;
        }

        private void OnReceiveMessageFromClient(ClientData client)
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[client.clientSocket.SendBufferSize];
                    int readBytes = client.clientSocket.Receive(buffer);

                    if (readBytes > 0)
                        _serverProtocol.ProcessPacketFromClient(new Packet(buffer));
                }
                catch (SocketException ex)
                {
                    Logger.Instance.Error($"Server error: {ex}");
                }
            }
        }

        private ClientData GetClientById(string uid)
        {
            return ActiveClients.FirstOrDefault(c => c.uid == uid);
        }
    }
}
