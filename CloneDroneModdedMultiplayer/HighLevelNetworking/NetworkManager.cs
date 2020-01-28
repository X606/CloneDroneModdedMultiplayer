using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloneDroneModdedMultiplayer.LowLevelNetworking;
using ModLibrary;
using InternalModBot;

namespace CloneDroneModdedMultiplayer.HighLevelNetworking
{
    public static class NetworkManager
    {
        static Dictionary<MessageID, NetworkMessageBase> _networkMessagesDictionary = new Dictionary<MessageID, NetworkMessageBase>();
		internal static Dictionary<string, ushort> ModUUIDToNetworkID = new Dictionary<string, ushort>();

        public static void AddNetworkMessage(NetworkMessageBase networkMessage, Mod mod)
        {
			if(!mod.IsNetworkedMod())
				throw new Exception("Mod must have the " + nameof(NetworkedModAttribute) + " attribute to register network messages from it");

			if(!NetworkingCore.IsConnected)
				throw new Exception("We must be connected to register a network message!");

			networkMessage.Owner = mod;

			if(_networkMessagesDictionary.ContainsKey(networkMessage.FullMessageID))
                throw new ArgumentException("A message with the id " + networkMessage.FullMessageID + " named \"" + _networkMessagesDictionary[networkMessage.FullMessageID].Name + " \" has already been defined. (new msg name \"" + networkMessage.Name + "\")", nameof(networkMessage));
			
            _networkMessagesDictionary.Add(networkMessage.FullMessageID, networkMessage);
        }

        public static void Init()
        {
			NetworkingCore.SERVER_OnClientConnected += SERVER_OnClientConnected;
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
			NetworkingCore.ScheduleForMainThread(delegate
			{
				MessageID messageID = new MessageID(arg2);
				if(_networkMessagesDictionary.TryGetValue(messageID, out NetworkMessageBase networkMessage))
				{
					byte[] buffer = new byte[arg2.Length-sizeof(ushort)];
					Buffer.BlockCopy(arg2, 2, buffer, 0, buffer.Length);

					networkMessage.OnReceived(buffer, isServer);
				}
				else
				{
					throw new NotImplementedException("No message with mod id " + messageID.ModID + ", msg id: " + messageID.MsgID + " found!");
				}
			});

		}

    }
}
