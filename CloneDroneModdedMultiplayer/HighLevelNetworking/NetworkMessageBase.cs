using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloneDroneModdedMultiplayer.LowLevelNetworking;
using ModLibrary;

namespace CloneDroneModdedMultiplayer.HighLevelNetworking
{
    public abstract class NetworkMessageBase
    {
		internal Mod Owner;

        public const int MAX_UNSAFE_PACKAGE_SIZE = NetworkingCore.UdpPackageSize-2;

        protected abstract ushort MsgID { get; }
        public abstract string Name { get; }
		public abstract MsgChannel Channel { get; }

        protected virtual void OnPackageReceivedServer(byte[] package)
        {
        }
        protected virtual void OnPackageReceivedClient(byte[] package)
        {
        }

		public MessageID FullMessageID
		{
			get
			{
				MessageID messageID = new MessageID();

				if(Owner is Main) // if the owner is the the modded multiplayer mod, set the ModID to 0
				{
					messageID.ModID = 0;
				}
				else
				{
					if(!NetworkManager.ModUUIDToNetworkID.TryGetValue(Owner.GetUniqueID(), out ushort value))
						throw new Exception("Invalid mod tried to register network message");

					messageID.ModID = value;
				}
				
				messageID.MsgID = MsgID;

				return messageID;
			}
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
			debug.Log("sending... length: " + data.Length);

            if(Channel == MsgChannel.Unsafe && data.Length != MAX_UNSAFE_PACKAGE_SIZE)
                throw new ArgumentException("The passed array must be " + MAX_UNSAFE_PACKAGE_SIZE + " long if the channel is set to unsafe", nameof(data));

			byte[] msg = createFullMsg(FullMessageID, data);
			debug.Log("full msg length: " + msg.Length);

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

        byte[] createFullMsg(MessageID msgID, byte[] package)
        {
            if(package.Length != MAX_UNSAFE_PACKAGE_SIZE && Channel == MsgChannel.Unsafe)
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
                fullMsg[i+sizeof(ushort)] = package[i];
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
