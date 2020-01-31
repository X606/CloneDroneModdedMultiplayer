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
			ThreadSafeDebug.Log("starting to get tcp data...");
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
			
			for(int i = 0; i < buffer.Length; i++)
			{
				ThreadSafeDebug.Log(i + "length: " + buffer[i]);
			}
			int length = BitConverter.ToInt32(buffer, 0);
			ThreadSafeDebug.Log("got data! length: " + length);
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

				for(int i = 0; i < bytesRead; i++)
				{
					ThreadSafeDebug.Log(i + "a: " + buffer[i]);
				}

				fileOffset += bytesRead;
				bytesLeftToReceive -= bytesRead;
			}

			for(int i = 0; i < outputData.Length; i++)
			{
				ThreadSafeDebug.Log(i + ": " + outputData[i]);
			}

			ThreadSafeDebug.Log("done getting data! length: " + length);

			//TcpConnection.Send(new byte[] { 1 }); // make sure other client has to wait for us to be done

			//ThreadSafeDebug.Log("100% done getting data!");

			return outputData;
		}
		public void TcpSend(byte[] data)
		{
			ThreadSafeDebug.Log("starting to send tcp data... length: " + data.Length);
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

			ThreadSafeDebug.Log("done sending tcp data... length: " + data.Length);

			//buffer = new byte[1];
			//TcpConnection.Receive(buffer);

			//ThreadSafeDebug.Log("done sending tcp data... end result: " + buffer[0]);
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
