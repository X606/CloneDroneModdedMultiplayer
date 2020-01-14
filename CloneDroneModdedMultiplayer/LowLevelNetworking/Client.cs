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
        public static Socket CLIENT_TcpServerConnection;
        public static Socket CLIENT_UdpServerConnection;
        public static EndPoint CLIENT_ServerEndpoint;

        static Queue<QueuedNetworkMessage> _queuedTcpNetworkMessages = new Queue<QueuedNetworkMessage>();
        static Queue<QueuedNetworkMessage> _queuedUdpNetworkMessages = new Queue<QueuedNetworkMessage>();

        public static bool StartClient(string ip, int port, Action callbackOnConnect = null)
        {
            CurrentClientType = ClientType.Client;

            IPAddress adress = IPAddress.Parse(ip);
            CLIENT_ServerEndpoint = new IPEndPoint(adress, port);

            CLIENT_TcpServerConnection = TcpConnect((IPEndPoint)CLIENT_ServerEndpoint);
            CLIENT_UdpServerConnection = UdpConnect((IPEndPoint)CLIENT_ServerEndpoint);

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
                while(CLIENT_TcpServerConnection.Available > 0)
                {
                    byte[] buffer = new byte[sizeof(int)];
                    CLIENT_TcpServerConnection.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    int length = BitConverter.ToInt32(buffer, 0);
                    buffer = new byte[length];
                    CLIENT_TcpServerConnection.Receive(buffer, 0, length, SocketFlags.None);
                    OnClientTcpMessage(buffer);
                }
                byte[] udpBuffer = new byte[UdpPackageSize];
                while(CLIENT_UdpServerConnection.Available > 0)
                {
                    CLIENT_UdpServerConnection.ReceiveFrom(udpBuffer, 0, UdpPackageSize, SocketFlags.None, ref CLIENT_ServerEndpoint);
                    OnClientUdpMessage(udpBuffer);
                }

                // sending messages
                lock(_queuedTcpNetworkMessages)
                {
                    while(_queuedTcpNetworkMessages.Count > 0)
                    {
                        QueuedNetworkMessage networkMessage = _queuedTcpNetworkMessages.Dequeue();
                        int msgLength = networkMessage.DataToSend.Length;
                        byte[] buffer = new byte[msgLength + sizeof(int)];
                        Buffer.BlockCopy(BitConverter.GetBytes(msgLength), 0, buffer, 0, sizeof(int)); // copies the bytes of msgLength into the first 4 slots of the buffer
                        Buffer.BlockCopy(networkMessage.DataToSend, 0, buffer, sizeof(int), msgLength);

                        CLIENT_TcpServerConnection.Send(buffer);
                    }
                }
                lock(_queuedUdpNetworkMessages)
                {
                    while(_queuedUdpNetworkMessages.Count > 0)
                    {
                        QueuedNetworkMessage networkMessage = _queuedUdpNetworkMessages.Dequeue();
                        CLIENT_TcpServerConnection.SendTo(networkMessage.DataToSend, CLIENT_ServerEndpoint);
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

        public static void OnClientTcpMessage(byte[] bytes)
        {

        }
        public static void OnClientUdpMessage(byte[] bytes)
        {

        }

        public static void SendClientTcpMessage(byte[] bytes)
        {
            lock(_queuedTcpNetworkMessages)
            {
                _queuedTcpNetworkMessages.Enqueue(new QueuedNetworkMessage()
                {
                    DataToSend = bytes
                });
            }
        }
        public static void SendClientUdpMessage(byte[] bytes)
        {
            if(bytes.Length != UdpPackageSize)
                throw new Exception("All Udp messages must be " + UdpPackageSize + " bytes long.");
            lock(_queuedUdpNetworkMessages)
            {
                _queuedUdpNetworkMessages.Enqueue(new QueuedNetworkMessage()
                {
                    DataToSend = bytes
                });
            }

        }

    }

    public class QueuedNetworkMessage
    {
        public byte[] DataToSend;
    }
}
