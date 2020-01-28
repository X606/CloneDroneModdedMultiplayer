using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace CloneDroneModdedMultiplayer.LowLevelNetworking
{
	public class ConnectedClient
	{
		public Socket TcpConnection;
		public Socket UdpConnection;

		public EndPoint EndPoint;

		public ConnectedClient(Socket tcpConnection, Socket updConnection, EndPoint endPoint)
		{
			TcpConnection=tcpConnection;
			UdpConnection=updConnection;
			EndPoint = endPoint;
		}

		public byte[] TcpRecive()
		{
			ThreadSafeDebug.Log("data before:" + TcpConnection.Available);

			byte[] buffer = new byte[sizeof(int)];
			TcpConnection.Receive(buffer, 0, buffer.Length, SocketFlags.None);

			for(int i = 0; i < buffer.Length; i++)
			{
				ThreadSafeDebug.Log(i + ": " + buffer[i]);
			}
			int length = BitConverter.ToInt32(buffer, 0);
			buffer = new byte[length];

			while(TcpConnection.Available < length) // wait for the full msg to come
				System.Threading.Thread.Sleep(1);

			TcpConnection.Receive(buffer, 0, length, SocketFlags.None);

			ThreadSafeDebug.Log("data left:" + TcpConnection.Available);

			System.IO.File.WriteAllBytes(UnityEngine.Application.persistentDataPath + "/testoutput.txt", buffer);

			return buffer;
		}
		public void TcpSend(byte[] data)
		{
			byte[] dataLengthBytes = BitConverter.GetBytes(data.Length);

			for(int i = 0; i < dataLengthBytes.Length; i++)
			{
				ThreadSafeDebug.Log(i + ": " + dataLengthBytes[i]);
			}
			TcpConnection.Send(dataLengthBytes, 0, sizeof(int), SocketFlags.None);
			TcpConnection.Send(data, 0, data.Length, SocketFlags.None);
		}

		public byte[] UdpRecive()
		{
			byte[] buffer = new byte[NetworkingCore.UdpPackageSize];
			UdpConnection.ReceiveFrom(buffer, NetworkingCore.UdpPackageSize, SocketFlags.None, ref EndPoint);
			return buffer;
		}
		public void UdpSend(byte[] data)
		{
			if(data.Length != NetworkingCore.UdpPackageSize)
				throw new Exception("All data sent over udp must be exactly " + NetworkingCore.UdpPackageSize + " bytes long");

			UdpConnection.SendTo(data, EndPoint);
		}

	}
}
