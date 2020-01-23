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
			byte[] buffer = new byte[sizeof(int)];
			TcpConnection.Receive(buffer, 0, buffer.Length, SocketFlags.None);
			int length = BitConverter.ToInt32(buffer, 0);
			buffer = new byte[length];
			TcpConnection.Receive(buffer, 0, length, SocketFlags.None);

			TcpConnection.Receive(buffer);

			return buffer;
		}
		public void TcpSend(byte[] data)
		{
			byte[] buffer = new byte[data.Length + sizeof(int)];
			byte[] dataLengthBytes = BitConverter.GetBytes(data.Length);
			Buffer.BlockCopy(dataLengthBytes, 0, buffer, 0, sizeof(int));
			Buffer.BlockCopy(data, 0, buffer, sizeof(int), data.Length);
			TcpConnection.Send(data);
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
