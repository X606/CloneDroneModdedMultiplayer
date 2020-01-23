using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloneDroneModdedMultiplayer.LowLevelNetworking;

namespace CloneDroneModdedMultiplayer.HighLevelNetworking
{
    public static class NetworkManager
    {
        static Dictionary<MessageID, NetworkMessageBase> _networkMessagesDictionary = new Dictionary<MessageID, NetworkMessageBase>();
        public static void AddNetworkMessage(NetworkMessageBase networkMessage)
        {
            if(_networkMessagesDictionary.ContainsKey(networkMessage.MsgID))
                throw new ArgumentException("A message with the id " + networkMessage.MsgID + " named \"" + _networkMessagesDictionary[networkMessage.MsgID].Name + " \" has already been defined. (new msg name \"" + networkMessage.Name + "\")", nameof(networkMessage));

            _networkMessagesDictionary.Add(networkMessage.MsgID, networkMessage);
        }

        public static void Init()
        {
			NetworkingCore.SERVER_CallbackOnClientConnected += SERVER_OnClientConnected;
			NetworkingCore.OnServerTcpMessage += (ConnectedClient client, byte[] data) => OnMessage(data, true);
			NetworkingCore.OnServerUdpMessage += (ConnectedClient client, byte[] data) => OnMessage(data, true);
			NetworkingCore.OnClientTcpMessage += (byte[] data) => OnMessage(data, false);
			NetworkingCore.OnClientUdpMessage += (byte[] data) => OnMessage(data, false);
		}

		static void SERVER_OnClientConnected(ConnectedClient obj)
		{
		}

		static void OnMessage(byte[] arg2, bool isServer)
		{
			MessageID messageID = new MessageID(arg2);
			if (_networkMessagesDictionary.TryGetValue(messageID, out NetworkMessageBase networkMessage))
			{
				byte[] buffer = new byte[arg2.Length-sizeof(ushort)];
				Buffer.BlockCopy(arg2, 2, buffer, 0, arg2.Length);

				networkMessage.OnReceived(buffer, isServer);
			} else
			{
				throw new NotImplementedException("No message with id " + messageID + " found!");
			}
		}

    }
}
