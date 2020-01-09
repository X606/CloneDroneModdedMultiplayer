using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLibrary;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using CloneDroneModdedMultiplayer.LowLevelNetworking;

namespace CloneDroneModdedMultiplayer
{
    public class Main : Mod
    {
        public override string GetModName() => "Clone drone modded multiplayer";
        public override string GetUniqueID() => "33f5eff2-e81f-444e-89d4-924b5c472616";

        public override void OnModLoaded()
        {
            if(LowLevelNetworkingMonoBehaviour.Instance == null)
                new GameObject(nameof(LowLevelNetworkingMonoBehaviour)).AddComponent<LowLevelNetworkingMonoBehaviour>();

            NetworkingCore.OnProcessMessageFromClientMainThread.Add(delegate(byte[] msg)
            {
                for(int i = 0; i < msg.Length; i++)
                {
                    debug.Log(msg[i]);
                }
            });
            NetworkingCore.OnProcessMessageFromServerMainThread.Add(delegate (byte[] msg)
            {
                for(int i = 0; i < msg.Length; i++)
                {
                    debug.Log(msg[i]);
                }
            });

        }

        public override void OnCommandRan(string command)
        {
            if (command == "startServer")
            {
                debug.Log("starting server...");
                NetworkingCore.StartServer(606);
            }
            if(command == "startClient")
            {
                debug.Log("starting client...");
                NetworkingCore.StartClient("localhost", 606);
            }
            if(command == "serverSendMsg")
            {
                debug.Log("sending msg server...");
                NetworkingCore.SERVER_SendMessageToAllClients(NetworkingCore.GenerateTestMessage());
            }
            if (command == "clientSendMsg")
            {
                debug.Log("sending msg client...");
                NetworkingCore.CLIENT_SendPackage(NetworkingCore.GenerateTestMessage());
            }
        }
    }
}
