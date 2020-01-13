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
        public static Socket CLIENT_ServerConnection;

        public static bool StartClient(string ip, int port, Action callbackOnConnect = null)
        {
            if(NetworkThread != null)
                return false;

            CurrentClientType = ClientType.Client;

            NetworkThread = new Thread(delegate() { CLIENT_NetworkThread(ip, port, callbackOnConnect); });
            NetworkThread.Start();

            return true;
        }

        public static void CLIENT_NetworkThread(string ip, int port, Action callbackOnConnect)
        {
            byte[] buffer = new byte[PACKAGE_SIZE];


            IPHostEntry hostEntry = Dns.GetHostEntry(ip);
            if (ip == "localhost")
            {
                hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            }

            foreach(IPAddress address in hostEntry.AddressList) // if there are more than 1 servers on the ip
            {
                IPEndPoint ipe = new IPEndPoint(address, port);
                Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                tempSocket.Connect(ipe);

                if(tempSocket.Connected)
                {
                    CLIENT_ServerConnection = tempSocket;
                    break;
                }
                else
                {
                    continue;
                }
            }
            if(CLIENT_ServerConnection == null)
            {
                ScheduleForMainThread(delegate
                {
                    debug.Log("Could not connect");
                });

                return;
            }
            

            Stopwatch stopwatch = new Stopwatch(); // used to measure the amount of time a "tick" takes

            if (callbackOnConnect != null)
                    callbackOnConnect();

            while(true)
            {
                stopwatch.Start();
                lock(CLIENT_ServerConnection)
                {
                    while(CLIENT_ServerConnection.Available > 0)
                    {
                        CLIENT_ServerConnection.Receive(buffer);
                        CLIENT_ProcessMessageFromServer(buffer);
                    }
                }

                stopwatch.Stop();
                if(stopwatch.ElapsedMilliseconds < 1000/TARGET_TPS) // to make sure we dont hog up the cpu more than we need to
                {
                    int milisecondsToWait = (1000/TARGET_TPS) - (int)stopwatch.ElapsedMilliseconds;
                    Thread.Sleep(milisecondsToWait);
                }
                stopwatch.Reset();
            }
        }
        public static void CLIENT_SendPackage(byte[] package)
        {
            if(package.Length != PACKAGE_SIZE)
                throw new ArgumentException("the passed package must have a size of " + PACKAGE_SIZE);

            new Thread(delegate ()
            {
                lock(CLIENT_ServerConnection)
                {
                    CLIENT_ServerConnection.Send(package, PACKAGE_SIZE, SocketFlags.None);
                }
            }).Start();
        }
        public static void CLIENT_ProcessMessageFromServer(byte[] package)
        {
            lock(OnProcessMessageFromClient)
            {
                foreach(var item in OnProcessMessageFromClient)
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
