using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace CloneDroneModdedMultiplayer.LowLevelNetworking
{
    public static partial class NetworkingCore
    {
        public const int PACKAGE_SIZE = 34;
        public const int TARGET_TPS = 60;
        
        public static Thread NetworkThread;
        public static List<Action<byte[]>> OnProcessMessageFromClient = new List<Action<byte[]>>();
        public static List<Action<byte[]>> OnProcessMessageFromServer = new List<Action<byte[]>>();

        public static List<Action<byte[]>> OnProcessMessageFromClientMainThread = new List<Action<byte[]>>();
        public static List<Action<byte[]>> OnProcessMessageFromServerMainThread = new List<Action<byte[]>>();

        public static ClientType CurrentClientType { get; private set; } = ClientType.Unknown;

        static List<Action> _scheduledForMainThread = new List<Action>();

        public static void ScheduleForMainThread(Action action)
        {
            lock(_scheduledForMainThread) {
                _scheduledForMainThread.Add(action);
            }
        }
        public static void CallAllActionsScheduled()
        {
            lock(_scheduledForMainThread)
            {
                foreach(Action item in _scheduledForMainThread)
                {
                    try
                    {
                        item();
                    } catch(Exception e)
                    {
                        UnityEngine.Debug.LogError(e.ToString());
                    }
                }
                _scheduledForMainThread.Clear();
            }
        }
        
        public static byte[] GenerateTestMessage()
        {
            byte[] output = new byte[PACKAGE_SIZE];
            for(int i = 2; i < PACKAGE_SIZE; i++)
            {
                output[i] = (byte)i;
            }
            output[0] = 0; output[1] = 0; // just to make 100% sure that the msgType is 0
            return output;
        }

    } 

    public enum ClientType
    {
        Unknown,
        Server,
        Client
    }
}
