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
		public readonly ushort ClientNetworkID;

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
			buffer = new byte[2048];
			byte[] outputData = new byte[length];

			int fileOffset = 0;
			int bytesLeftToReceive = length;
			while(bytesLeftToReceive > 0) // recives the map in chunks so large msgs should be supported
			{
				int bytesRead = TcpConnection.Receive(buffer);

				int bytesToCopy = Math.Min(bytesRead, bytesLeftToReceive);

				Buffer.BlockCopy(buffer, 0, outputData, fileOffset, bytesToCopy);

				fileOffset += bytesRead;
				bytesLeftToReceive -= bytesRead;
			}

			TcpConnection.Send(new byte[] { 1 }); // make sure other client has to wait for us to be done

			return outputData;
		}
		public void TcpSend(byte[] data)
		{
			byte[] dataLengthBytes = BitConverter.GetBytes(data.Length);
			
			TcpConnection.Send(dataLengthBytes, 0, sizeof(int), SocketFlags.None);

			byte[] buffer = new byte[2048];

			int fileOffset = 0;
			int bytesLeft = data.Length;

			while(bytesLeft > 0) // send map in chunks so large msgs should be supported
			{
				int bytesToSend = Math.Min(bytesLeft, 2048);

				TcpConnection.Send(data, fileOffset, bytesToSend, SocketFlags.None);

				fileOffset += bytesToSend;
				bytesLeft -= bytesToSend;
			}

			TcpConnection.Send(data, 0, data.Length, SocketFlags.None); // wait for other client to recive all data

			buffer = new byte[1];
			TcpConnection.Receive(buffer);
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
