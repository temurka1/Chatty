using Chatty.Protocol;
using Chatty.Protocol.Server;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Chatty.Server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Enter port value = ");
            string port = Console.ReadLine();

            Int32.TryParse(port, out int portValue);

            IServer server = new ChattyServer();
            server.Start(new ChattyServerProtocol(server), GetIP4Address(), portValue);

            Console.ReadKey();
        }

        public static string GetIP4Address() => Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
    }
}
