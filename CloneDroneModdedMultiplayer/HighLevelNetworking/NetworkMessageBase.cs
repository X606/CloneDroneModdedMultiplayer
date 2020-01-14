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
        public const int MAX_PACKAGE_SIZE = NetworkingCore.UdpPackageSize-2;

        public abstract byte MsgID { get; }
        public abstract string Name { get; }

        protected virtual void OnPackageReceivedServer(byte[] package)
        {
        }
        protected virtual void OnPackageReceivedClient(byte[] package)
        {
        }

        public void OnReceived(byte[] package, bool isServer)
        {
            if (package.Length == NetworkingCore.UdpPackageSize) // if this is a full package and the msgtype prefix hasnt been removed, remove the msgtype prefix
            {
                byte[] tempPackage = new byte[MAX_PACKAGE_SIZE];
                for(int i = 0; i < MAX_PACKAGE_SIZE; i++)
                {
                    tempPackage[i] = package[i+2];
                }
                package = tempPackage;
            }

            if(package.Length != MAX_PACKAGE_SIZE)
                throw new ArgumentException("The passed array must be either " + MAX_PACKAGE_SIZE + " or " + NetworkingCore.UdpPackageSize + " long.", nameof(package));

            if(isServer)
            {
                OnPackageReceivedServer(package);
            } else
            {
                OnPackageReceivedClient(package);
            }
        }
        public void Send(byte[] data)
        {
            if(data.Length != MAX_PACKAGE_SIZE)
                throw new ArgumentException("The passed array must be " + MAX_PACKAGE_SIZE + " long.", nameof(data));

            byte[] fullData = createFullMsg(MsgID, data);

            ClientType clientType = NetworkingCore.CurrentClientType;
            switch(clientType)
            {
                case ClientType.Server:
                    NetworkingCore.SERVER_SendMessageToAllClients(fullData);
                break;
                case ClientType.Client:
                NetworkingCore.CLIENT_SendPackage(fullData);
                break;

                case ClientType.Unknown:
                default:
                    throw new Exception("We are nither a server or a client");
            }
        }

        public short CompleteMsgID
        {
            get
            {
                return BitConverter.ToInt16(new byte[] { 0, MsgID }, 0);
            }
        }

        static byte[] createFullMsg(short msgID, byte[] package)
        {
            if(package.Length != MAX_PACKAGE_SIZE)
                throw new ArgumentException("The passed array must be " + MAX_PACKAGE_SIZE + " long.", nameof(package));

            byte[] fullMsg = new byte[NetworkingCore.UdpPackageSize];
            
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException("non little endian systems not supported for now"); // TODO: Support non little endian systems

            byte[] prefix = BitConverter.GetBytes(msgID);
            for(int i = 0; i < sizeof(short); i++)
            {
                fullMsg[i] = prefix[i];
            }
            for(int i = 2; i < NetworkingCore.UdpPackageSize; i++)
            {
                fullMsg[i] = package[i-2];
            }
            return fullMsg;
        }

    }
}
