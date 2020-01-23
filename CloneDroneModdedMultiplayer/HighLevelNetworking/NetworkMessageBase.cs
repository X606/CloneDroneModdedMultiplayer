using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloneDroneModdedMultiplayer.LowLevelNetworking;

namespace CloneDroneModdedMultiplayer.HighLevelNetworking
{
    public abstract class NetworkMessageBase
    {
        public const int MAX_UNSAFE_PACKAGE_SIZE = NetworkingCore.UdpPackageSize-2;

        public abstract MessageID MsgID { get; }
        public abstract string Name { get; }
		public abstract MsgChannel Channel { get; }

        protected virtual void OnPackageReceivedServer(byte[] package)
        {
        }
        protected virtual void OnPackageReceivedClient(byte[] package)
        {
        }

        public void OnReceived(byte[] package, bool isServer)
        {
            if (NetworkingCore.CurrentClientType == ClientType.Client)
			{
				OnPackageReceivedClient(package);
			}
			else if(NetworkingCore.CurrentClientType == ClientType.Server)
			{
				OnPackageReceivedServer(package);
			}

		}
        public void Send(byte[] data)
        {
            if(Channel == MsgChannel.Unsafe && data.Length != MAX_UNSAFE_PACKAGE_SIZE)
                throw new ArgumentException("The passed array must be " + MAX_UNSAFE_PACKAGE_SIZE + " long if the channel is set to unsafe", nameof(data));

			byte[] msg = createFullMsg(MsgID, data);

			if (NetworkingCore.CurrentClientType == ClientType.Client)
			{
				if (Channel == MsgChannel.Safe)
				{
					NetworkingCore.SendClientTcpMessage(msg);
				}
				else if (Channel == MsgChannel.Unsafe)
				{
					NetworkingCore.SendClientUdpMessage(msg);
				}
			}
			else if (NetworkingCore.CurrentClientType == ClientType.Server)
			{
				if(Channel == MsgChannel.Safe)
				{
					NetworkingCore.SendServerTcpMessage(msg);
				}
				else if(Channel == MsgChannel.Unsafe)
				{
					NetworkingCore.SendServerUdpMessage(msg);
				}
			}

        }

        static byte[] createFullMsg(MessageID msgID, byte[] package)
        {
            if(package.Length != MAX_UNSAFE_PACKAGE_SIZE)
                throw new ArgumentException("The passed array must be " + MAX_UNSAFE_PACKAGE_SIZE + " long.", nameof(package));

            byte[] fullMsg = new byte[package.Length + sizeof(ushort)];
            
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException("non little endian systems not supported for now"); // TODO: Support non little endian systems

            byte[] prefix = BitConverter.GetBytes(msgID.RawValue);
            for(int i = 0; i < sizeof(ushort); i++)
            {
                fullMsg[i] = prefix[i];
            }
            for(int i = 0; i < package.Length; i++)
            {
                fullMsg[i] = package[i+sizeof(ushort)];
            }
            return fullMsg;
        }

    }
	public enum MsgChannel
	{
		/// <summary>
		/// Uses the tcp protocol that makes sure all messages get to the other end.
		/// </summary>
		Safe,
		/// <summary>
		/// Uses the udp protocol that is faster than tcp but does NOT make sure the messages get to the other end.
		/// </summary>
		Unsafe
	}

}
