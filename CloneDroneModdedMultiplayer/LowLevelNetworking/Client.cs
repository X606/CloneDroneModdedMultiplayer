using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ModLibrary;
using System.Diagnostics;

namespace CloneDroneModdedMultiplayer.LowLevelNetworking
{
    public static partial class NetworkingCore
    {
		public static volatile ConnectedClient CLIENT_ServerConnection;

		public static event Action<byte[]> OnClientTcpMessage;
		public static event Action<byte[]> OnClientUdpMessage;

		static Queue<QueuedNetworkMessage> _CLIENT_queuedTcpNetworkMessages = new Queue<QueuedNetworkMessage>();
        static Queue<QueuedNetworkMessage> _CLIENT_queuedUdpNetworkMessages = new Queue<QueuedNetworkMessage>();

        public static bool StartClient(string ip, int port)
        {
            CurrentClientType = ClientType.Client;

            IPAddress adress = IPAddress.Parse(ip);
            EndPoint serverEndpoint = new IPEndPoint(adress, port);

            Socket tcpSocket = TcpConnect((IPEndPoint)serverEndpoint);
            Socket UdpSocket = UdpConnect((IPEndPoint)serverEndpoint);
			CLIENT_ServerConnection = new ConnectedClient(tcpSocket, UdpSocket, serverEndpoint);

            new Thread(CLIENT_Mainloop).Start();

            return true;
        }

        public static Socket TcpConnect(IPEndPoint endPoint)
        {
            Socket connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            connection.Connect(endPoint);

            if(!connection.Connected)
                throw new Exception("Could not connect");

            return connection;
        }
        public static Socket UdpConnect(IPEndPoint endPoint)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            return socket;
        }

        public static void CLIENT_Mainloop()
        {
            Stopwatch stopwatch = new Stopwatch();
            while(true)
            {
                stopwatch.Start();
                
                // getting messages
                while(CLIENT_ServerConnection.TcpConnection.Available > 0)
                {
					byte[] buffer = CLIENT_ServerConnection.TcpRecive();
                    OnClientTcpMessage(buffer);
                }
                
                while(CLIENT_ServerConnection.UdpConnection.Available > 0)
                {
					byte[] buffer = CLIENT_ServerConnection.UdpRecive();
					OnClientUdpMessage(buffer);
                }

                // sending messages
                lock(_CLIENT_queuedTcpNetworkMessages)
                {
                    while(_CLIENT_queuedTcpNetworkMessages.Count > 0)
                    {
                        QueuedNetworkMessage networkMessage = _CLIENT_queuedTcpNetworkMessages.Dequeue();
						CLIENT_ServerConnection.TcpSend(networkMessage.DataToSend);
                    }
                }
                lock(_CLIENT_queuedUdpNetworkMessages)
                {
                    while(_CLIENT_queuedUdpNetworkMessages.Count > 0)
                    {
                        QueuedNetworkMessage networkMessage = _CLIENT_queuedUdpNetworkMessages.Dequeue();
						CLIENT_ServerConnection.UdpSend(networkMessage.DataToSend);
                    }
                }

                stopwatch.Stop();
                int time = (int)stopwatch.ElapsedMilliseconds;
                if (time < 1000/TARGET_TPS)
                {
                    Thread.Sleep((1000/TARGET_TPS)-time);
                }
                stopwatch.Reset();
            }
        }

        public static void SendClientTcpMessage(byte[] bytes)
        {
            lock(_CLIENT_queuedTcpNetworkMessages)
            {
				_CLIENT_queuedTcpNetworkMessages.Enqueue(new QueuedNetworkMessage()
				{
					DataToSend = bytes
                });
            }
        }
        public static void SendClientUdpMessage(byte[] bytes)
        {
            if(bytes.Length != UdpPackageSize)
                throw new Exception("All Udp messages must be " + UdpPackageSize + " bytes long.");

            lock(_CLIENT_queuedUdpNetworkMessages)
            {
                _CLIENT_queuedUdpNetworkMessages.Enqueue(new QueuedNetworkMessage()
                {
                    DataToSend = bytes
                });
            }

        }

    }
}
