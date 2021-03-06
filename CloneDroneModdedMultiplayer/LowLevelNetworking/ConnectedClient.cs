﻿using System;
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
		public ushort ClientNetworkID { internal set; get; }

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
			byte[] lengthBytes = new byte[sizeof(int)];
			byte[] buffer = new byte[sizeof(int)];
			int fileOffset = 0;
			int bytesLeftToReceive = buffer.Length;
			while(bytesLeftToReceive > 0)
			{
				int bytesLeft = Math.Min(4, bytesLeftToReceive);

				int countGotten = TcpConnection.Receive(buffer, 0, bytesLeft, SocketFlags.None);

				Buffer.BlockCopy(buffer, 0, lengthBytes, fileOffset, countGotten);

				fileOffset += countGotten;
				bytesLeftToReceive -= countGotten;
			}
			int length = BitConverter.ToInt32(buffer, 0);
			
			buffer = new byte[2048];
			byte[] outputData = new byte[length];

			fileOffset = 0;
			bytesLeftToReceive = length;
			while(bytesLeftToReceive > 0) // recives the map in chunks so large msgs should be supported
			{
				int bytesLeft = Math.Min(2048, bytesLeftToReceive);

				int bytesRead = TcpConnection.Receive(buffer, 0, bytesLeft, SocketFlags.None);

				//int bytesToCopy = Math.Min(bytesRead, bytesLeftToReceive);

				Buffer.BlockCopy(buffer, 0, outputData, fileOffset, bytesRead);

				fileOffset += bytesRead;
				bytesLeftToReceive -= bytesRead;
			}
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
