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
        public const int UdpPackageSize = 34;
        public const int TARGET_TPS = 60;

        public static ClientType CurrentClientType { get; private set; } = ClientType.Unknown;
		public static bool IsConnected => CurrentClientType != ClientType.Unknown;

        static List<Action> _scheduledForMainThread = new List<Action>();

        public static void ScheduleForMainThread(Action action)
        {
            lock(_scheduledForMainThread) {
                _scheduledForMainThread.Add(action);
            }
        }
        internal static void CallAllActionsScheduled()
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

    }

	public class QueuedNetworkMessage
	{
		public byte[] DataToSend;
		public int? TargetConnection = null;
	}

}
