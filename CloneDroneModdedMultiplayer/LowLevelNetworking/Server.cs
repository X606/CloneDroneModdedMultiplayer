using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace CloneDroneModdedMultiplayer.LowLevelNetworking
{
    public static partial class NetworkingCore
    {
        public static List<Socket> SERVER_ConnectedClients = new List<Socket>();

        public static bool StartServer(int port, Action<Socket> callbackOnClientConnected = null)
        {
            CurrentClientType = ClientType.Server;



            return true;
        }


    }

    public class ConnectedClient
    {
        public Socket TcpConnection;
        public Socket UpdConnection;

        public ConnectedClient(Socket tcpConnection, Socket updConnection)
        {
            TcpConnection=tcpConnection;
            UpdConnection=updConnection;
        }

    }
}
