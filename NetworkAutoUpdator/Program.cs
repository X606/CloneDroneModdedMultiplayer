using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace NetworkAutoUpdator
{
	class Program
	{
		public const int PORT = 8600;

		static void Main(string[] args)
		{
			string exepath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			string directory = Path.GetDirectoryName(exepath);
			string fullPathToPathCfg = directory + "/path.cfg";

			bool isSending = args.Contains("-s");
			string path = null;
			
			if(File.Exists(fullPathToPathCfg))
			{
				path = File.ReadAllText(fullPathToPathCfg);
			}
			

			string localPath = null;

			foreach(string arg in args)
			{
				if (arg.StartsWith("-p"))
				{
					path = arg.Substring(2);
					File.WriteAllText(fullPathToPathCfg, path);
				}
				if (arg.StartsWith("-lp"))
				{
					localPath = arg.Substring(3);
				}
			}

			if(path == null && !isSending)
			{
				do
				{
					Console.WriteLine("Please input your clone drone mods path:");
					path = Console.ReadLine();
				} while(!Directory.Exists(path));

				File.WriteAllText(fullPathToPathCfg, path);

			}
			
			if(isSending)
			{
				byte[] fileData = File.ReadAllBytes(localPath);
				
				File.WriteAllBytes(path + "/" + Path.GetFileName(localPath), fileData);

				Console.WriteLine("Updated local version!");

				Send(localPath, directory);
			} else
			{
				Listen(path);
			}

		}

		static void Send(string inputFile, string localPath)
		{
			string[] ips = File.ReadAllLines(localPath + "/ips.cfg");
			foreach(string ip in ips)
			{
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				EndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), PORT);
				socket.Connect(endPoint);

				string filename = Path.GetFileName(inputFile);

				Send(socket, Encoding.UTF8.GetBytes(filename));
				Send(socket, File.ReadAllBytes(inputFile));

				Console.WriteLine("Sent to \"" + ip + "\"");
			}

			Console.WriteLine("Sent to all ips!");
		}
		static void Listen(string path)
		{
			EndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT);
			Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			listener.Bind(endPoint);
			listener.Listen(10);

			while(true)
			{
				Socket connection = listener.Accept();

				string fileName = Encoding.UTF8.GetString(Recive(connection));
				byte[] data = Recive(connection);

				File.WriteAllBytes(path + "/" + fileName, data);

				Console.WriteLine(DateTime.Now.ToLocalTime() + ": Downloaded \"" + fileName + "\"");
				connection.Close();
			}
		}

		static byte[] Recive(Socket connection)
		{
			byte[] lengthBytesBuffer = new byte[sizeof(int)];
			connection.Receive(lengthBytesBuffer);

			int length = BitConverter.ToInt32(lengthBytesBuffer, 0);

			byte[] finalOutput = new byte[length];
			byte[] buffer = new byte[2048];

			int bytesLeftToReceive = length;
			int fileOffset = 0;
			while(bytesLeftToReceive > 0)
			{
				int bytesRead = connection.Receive(buffer);

				int bytesToWrite = Math.Min(bytesRead, bytesLeftToReceive);

				Buffer.BlockCopy(buffer, 0, finalOutput, fileOffset, bytesToWrite);

				bytesLeftToReceive -= bytesToWrite;
				fileOffset += bytesToWrite;
			}

			connection.Send(new byte[] { 1 });

			return finalOutput;
		}
		static void Send(Socket connection, byte[] data)
		{
			connection.Send(BitConverter.GetBytes(data.Length));

			int bytesLeft = data.Length;
			int fileOffset = 0;
			while(bytesLeft > 0)
			{
				int bytesToWrite = Math.Min(bytesLeft, 2048);

				connection.Send(data, fileOffset, bytesToWrite, SocketFlags.None);

				fileOffset += bytesToWrite;
				bytesLeft -= bytesToWrite;
			}

			byte[] buffer = new byte[1];
			connection.Receive(buffer);
		}

	}
}
