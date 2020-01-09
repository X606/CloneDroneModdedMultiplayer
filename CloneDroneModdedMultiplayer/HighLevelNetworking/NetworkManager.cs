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
        static Dictionary<short, NetworkMessageBase> _networkMessagesDictionary = new Dictionary<short, NetworkMessageBase>();
        public static void AddNetworkMessage(NetworkMessageBase networkMessage)
        {
            if(_networkMessagesDictionary.ContainsKey(networkMessage.MsgID))
                throw new ArgumentException("A message with the id " + networkMessage.MsgID + " named \"" + _networkMessagesDictionary[networkMessage.MsgID].Name + " \" has already been defined. (new msg name \"" + networkMessage.Name + "\")", nameof(networkMessage));

            _networkMessagesDictionary.Add(networkMessage.MsgID, networkMessage);
        }

        public static void Init()
        {
            NetworkingCore.OnProcessMessageFromClientMainThread.Add(delegate(byte[] data)
            {
                OnPackageRecived(data, false);
            });
            NetworkingCore.OnProcessMessageFromServerMainThread.Add(delegate (byte[] data)
            {
                OnPackageRecived(data, true);
            });
        }

        public static void OnPackageRecived(byte[] package, bool isServer)
        {
            short msgID = BitConverter.ToInt16(package, 0); // get the msgID of the recived package

            if(!_networkMessagesDictionary.ContainsKey(msgID))
                throw new NotImplementedException("No message with id " + msgID + " found!");

            NetworkMessageBase messageBase = _networkMessagesDictionary[msgID];

            messageBase.OnReceived(package, isServer); // on recived will remove the id from the front
        }

    }
}
