using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneDroneModdedMultiplayer.HighLevelNetworking
{
    public class General
    {
        public Dictionary<short, NetworkMessageBase> NetworkMessagesDict = new Dictionary<short, NetworkMessageBase>();
        List<NetworkMessageBase> _networkMessages = new List<NetworkMessageBase>();

        public void RefreshNetworkMessagesDict()
        {
            NetworkMessagesDict.Clear();
            foreach(NetworkMessageBase networkMessage in _networkMessages)
            {
                if(NetworkMessagesDict.ContainsKey(networkMessage.MsgID))
                    throw new Exception("there are 2 or more networkMessages with the same id. network message names: \"" + NetworkMessagesDict[networkMessage.MsgID].Name + "\" and \"" + networkMessage.Name + "\"");

                NetworkMessagesDict.Add(networkMessage.MsgID, networkMessage);
            }
        }

    }
}
