﻿using System;
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
        public static List<ConnectedClient> SERVER_ConnectedClients = new List<ConnectedClient>();

		/// <summary>NOTE: This will run on a seperate thread</summary>
		public static event Action<ConnectedClient, byte[], ushort> OnServerTcpMessage;
		/// <summary>NOTE: This will run on a seperate thread</summary>
		public static event Action<ConnectedClient, byte[], ushort> OnServerUdpMessage;

		/// <summary>NOTE: This will run on a seperate thread</summary>
		public static event Action<ConnectedClient> SERVER_OnClientConnected;

		static Queue<QueuedNetworkMessage> _SERVER_queuedTcpNetworkMessages = new Queue<QueuedNetworkMessage>();
		static Queue<QueuedNetworkMessage> _SERVER_queuedUdpNetworkMessages = new Queue<QueuedNetworkMessage>();

		public static bool StartServer(int port)
        {
            CurrentClientType = ClientType.Host;

			new Thread(delegate() { SERVER_AcceptConnectionsThread(port); }).Start(); // start accept thread
			new Thread(SERVER_Mainloop).Start(); // start main server network thread

            return true;
        }

		static void SERVER_AcceptConnectionsThread(int port)
		{
			EndPoint endpoint = new IPEndPoint(IPAddress.Any, port);
			Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			tcpListener.Bind(endpoint);
			tcpListener.Listen(10);

			while(true)
			{
				Socket connection = tcpListener.Accept();
				Socket udpConnection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				ConnectedClient client = new ConnectedClient(connection, udpConnection, connection.RemoteEndPoint);
				client.ClientNetworkID = getNextClientNetworkID();

				lock(SERVER_ConnectedClients)
				{
					SERVER_ConnectedClients.Add(client);
				}
				SERVER_OnClientConnected(client);

			}
			
		}
		public static void SERVER_Mainloop()
		{
			Stopwatch stopwatch = new Stopwatch();
			while(true)
			{
				stopwatch.Start();

				lock(SERVER_ConnectedClients)
				{
					foreach(ConnectedClient clientConnection in SERVER_ConnectedClients)
					{
						lock(clientConnection)
						{
							while(clientConnection.TcpConnection.Available > 0)
							{
								byte[] buffer = clientConnection.TcpRecive();
								OnServerTcpMessage(clientConnection, buffer, clientConnection.ClientNetworkID);
							}
							EndPoint endPoint = clientConnection.TcpConnection.RemoteEndPoint;
							while(clientConnection.UdpConnection.Available > 0)
							{
								byte[] buffer = clientConnection.UdpRecive();
								OnServerUdpMessage(clientConnection, buffer, clientConnection.ClientNetworkID);
							}
							
						}
					}
					lock(_SERVER_queuedTcpNetworkMessages)
					{
						while(_SERVER_queuedTcpNetworkMessages.Count > 0)
						{
							var msg = _SERVER_queuedTcpNetworkMessages.Dequeue();
							foreach(ConnectedClient clientConnection in SERVER_ConnectedClients)
							{
								if(msg.TargetConnection == null || msg.TargetConnection.Value == clientConnection.ClientNetworkID)
									clientConnection.TcpSend(msg.DataToSend);
							}
						}
					}
					lock(_SERVER_queuedUdpNetworkMessages)
					{
						while(_SERVER_queuedUdpNetworkMessages.Count > 0)
						{
							var msg = _SERVER_queuedUdpNetworkMessages.Dequeue();
							foreach(ConnectedClient clientConnection in SERVER_ConnectedClients)
							{
								if(msg.TargetConnection == null || msg.TargetConnection.Value == clientConnection.ClientNetworkID)
									clientConnection.UdpSend(msg.DataToSend);
							}
						}
					}

				}

				stopwatch.Stop();
				int time = (int)stopwatch.ElapsedMilliseconds;
				if(time < 1000/TARGET_TPS)
				{
					Thread.Sleep((1000/TARGET_TPS)-time);
				}
				stopwatch.Reset();
			}
		}

		public static void SendServerTcpMessage(byte[] bytes, ushort? target = null)
		{
			lock(_SERVER_queuedTcpNetworkMessages)
			{
				_SERVER_queuedTcpNetworkMessages.Enqueue(new QueuedNetworkMessage()
				{
					DataToSend = bytes,
					TargetConnection = target
				});
			}
		}
		public static void SendServerUdpMessage(byte[] bytes, ushort? target = null)
		{
			if(bytes.Length != UdpPackageSize)
				throw new Exception("All Udp messages must be " + UdpPackageSize + " bytes long.");
			lock(_SERVER_queuedUdpNetworkMessages)
			{
				_SERVER_queuedUdpNetworkMessages.Enqueue(new QueuedNetworkMessage()
				{
					DataToSend = bytes,
					TargetConnection = target
				});
			}

		}

		static ushort _currentClientNetworkID = 1;
		static ushort getNextClientNetworkID()
		{
			ushort selectedId = _currentClientNetworkID;
			_currentClientNetworkID++;
			return selectedId;
		}

	}

    
}
