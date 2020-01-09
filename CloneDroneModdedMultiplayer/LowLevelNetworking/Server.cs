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
        public static Thread SERVER_AcceptThread;

        public static bool StartServer(int port)
        {
            if(NetworkThread != null || SERVER_AcceptThread != null)
                return false;

            CurrentClientType = ClientType.Server;

            SERVER_AcceptThread = new Thread(delegate() { SERVER_acceptThread(port); });
            NetworkThread = new Thread(SERVER_NetworkThread);

            SERVER_AcceptThread.Start();
            NetworkThread.Start();

            return true;
        }

        static void SERVER_acceptThread(int port)
        {
            byte[] bytes = new byte[PACKAGE_SIZE];

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(10);
            

            while(true)
            {
                Socket handler = listener.Accept();

                handler.Receive(bytes);
                handler.Send(bytes);

                lock(SERVER_ConnectedClients)
                {
                    SERVER_ConnectedClients.Add(handler);
                }
            }
        }
        static void SERVER_NetworkThread()
        {
            byte[] buffer = new byte[PACKAGE_SIZE];
            Stopwatch stopwatch = new Stopwatch(); // used to m the amount of time a "tick" takes

            while(true) {
                stopwatch.Start();
                lock(SERVER_ConnectedClients)
                {
                    foreach(Socket client in SERVER_ConnectedClients)
                    {
                        while(client.Available > 0)
                        {
                            client.Receive(buffer, PACKAGE_SIZE, SocketFlags.None);
                            SERVER_ProcessMessageFromClient(buffer);
                        }

                    }
                }
                stopwatch.Stop();
                if (stopwatch.ElapsedMilliseconds < 1000/TARGET_TPS) // to make sure we dont hog up the cpu more than we need to
                {
                    int milisecondsToWait = (1000/TARGET_TPS) - (int)stopwatch.ElapsedMilliseconds;
                    Thread.Sleep(milisecondsToWait);
                }
                stopwatch.Reset();
            }
            
        }

        public static void SERVER_SendMessageToAllClients(byte[] package)
        {
            if(package.Length != PACKAGE_SIZE)
                throw new ArgumentException("the passed package must have a size of " + PACKAGE_SIZE);

            new Thread(delegate ()
            {
                lock(SERVER_ConnectedClients)
                {
                    foreach(Socket client in SERVER_ConnectedClients)
                    {
                        client.Send(package, PACKAGE_SIZE, SocketFlags.None);
                    }
                }
            }).Start();
            
        }
        static void SERVER_ProcessMessageFromClient(byte[] package)
        {

            lock(OnProcessMessageFromServer)
            {
                foreach(var item in OnProcessMessageFromServer)
                {
                    item(package);
                }
            }
            ScheduleForMainThread(delegate
            {
                lock(OnProcessMessageFromServerMainThread)
                {
                    foreach(var item in OnProcessMessageFromServerMainThread)
                    {
                        item(package);
                    }
                }
            });
        }


    }
}
