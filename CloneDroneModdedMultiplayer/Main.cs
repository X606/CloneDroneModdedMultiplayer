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
using CloneDroneModdedMultiplayer.HighLevelNetworking;
using CloneDroneModdedMultiplayer.UI;
using CloneDroneModdedMultiplayer.Patches;
using CloneDroneModdedMultiplayer.Internal;

namespace CloneDroneModdedMultiplayer
{
    public class Main : Mod
    {
        static bool _hasInjected = false;

        public const GameMode MODDED_MULTIPLAYER_TEXT_GAMEMODE = (GameMode)2526;

        public override string GetModName() => "Clone drone modded multiplayer";
        public override string GetUniqueID() => "33f5eff2-e81f-444e-89d4-924b5c472616";

        public override void OnModLoaded()
        {
            if(LowLevelNetworkingMonoBehaviour.Instance == null)
                new GameObject(nameof(LowLevelNetworkingMonoBehaviour)).AddComponent<LowLevelNetworkingMonoBehaviour>();

            NetworkManager.Init();

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
        public override void OnModEnabled()
        {
            if(!_hasInjected)
            {
                Injector.InjectPrefix<LevelManager, GetLevelDescriptionsPatches>("getLevelDescriptions", "Prefix", this);
                Injector.InjectPostfix<LevelManager, GetLevelDescriptionsPatches>("getLevelDescriptions", "Postfix", this);

                Injector.InjectPostfix<GameDataManager, GetCurrentGameDataPatches>("getCurrentGameData", "Postpatch", this);
                
                Injector.InjectPostfix<GameModeManager, GameModeManagerAllowsLevelsWithNoEnemiesPatch>("AllowsLevelsWithNoEnemies", "PostPatch", this);

                _hasInjected = true;
            }

        }
        public override void OnModRefreshed()
        {
            ModdedMultiplayerUIManager.InitUI();
        }

        public override void OnCommandRan(string command)
        {
            string[] subCommand = command.Split(" ".ToCharArray());
            if(subCommand[0].ToLower() == "connect")
            {
                debug.Log("starting client...");

                ServerRunner.StartClient(subCommand[1], 606);

                /*NetworkingCore.StartClient(subCommand[1], 606, delegate
                {
                    NetworkingCore.ScheduleForMainThread(delegate
                    {
                        debug.Log("connected!");
                    });
                });*/

                
                
                //ServerRunner.StartClient(subCommand[1]);
            }
            if (subCommand[0].ToLower() == "startserver")
            {
                debug.Log("starting server...");

                NetworkingCore.StartServer(606, delegate(Socket socket)
                {
                    NetworkingCore.ScheduleForMainThread(delegate
                    {
                        debug.Log("client Connected!");
                    });
                });
            }

        }
        public override void OnLanugageChanged(string newLanguageID, Dictionary<string, string> localizationDictionary)
        {
            localizationDictionary.Add("moddedmultiplayermainmenumutton", "Play Modded Multiplayer");
            localizationDictionary.Add("test", "test");
            localizationDictionary.Add("test name", "test name");
        }
    }
}
