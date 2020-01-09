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
        public const int MAX_PACKAGE_SIZE = NetworkingCore.PACKAGE_SIZE-2;
        public abstract short MsgID { get; }
        public abstract string Name { get; }
        protected abstract void OnPackageRecived(byte[] package);

        public void OnRecived(byte[] package)
        {
            if (package.Length == NetworkingCore.PACKAGE_SIZE)
            {
                byte[] tempPackage = new byte[MAX_PACKAGE_SIZE];
                for(int i = 0; i < MAX_PACKAGE_SIZE; i++)
                {
                    tempPackage[i] = package[i+2];
                }
                package = tempPackage;
            }

            if(package.Length != MAX_PACKAGE_SIZE)
                throw new ArgumentException("The passed array must be either " + MAX_PACKAGE_SIZE + " or " + NetworkingCore.PACKAGE_SIZE + " long.", nameof(package));

            OnPackageRecived(package);
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

        private static byte[] createFullMsg(short msgID, byte[] package)
        {
            if(package.Length != MAX_PACKAGE_SIZE)
                throw new ArgumentException("The passed array must be " + MAX_PACKAGE_SIZE + " long.", nameof(package));

            byte[] fullMsg = new byte[NetworkingCore.PACKAGE_SIZE];
            
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException("non little endian systems not supported for now"); // TODO: Support non little endian systems

            byte[] prefix = BitConverter.GetBytes(msgID);
            for(int i = 0; i < sizeof(short); i++)
            {
                fullMsg[i] = prefix[i];
            }
            for(int i = 2; i < NetworkingCore.PACKAGE_SIZE; i++)
            {
                fullMsg[i] = package[i-2];
            }
            return fullMsg;
        }

    }
}
