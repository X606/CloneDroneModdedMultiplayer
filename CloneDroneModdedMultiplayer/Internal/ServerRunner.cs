using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModLibrary;
using System.IO;
using Newtonsoft.Json;
using System.Net.Sockets;
using CloneDroneModdedMultiplayer.LowLevelNetworking;

namespace CloneDroneModdedMultiplayer.Internal
{
    public static class ServerRunner
    {
        public static GameData CurrentGameData;

        public static List<MultiplayerPlayer> Players = new List<MultiplayerPlayer>();

        public static void SpawnPlayer(Vector3 spawnPointPosition, bool assignMainPlayer)
        {
            GameObject spawnPoint = new GameObject();
            spawnPoint.transform.position = spawnPointPosition;

            FirstPersonMover spawnedPlayer = GameFlowManager.Instance.SpawnPlayer(spawnPoint.transform, true, assignMainPlayer);
            Players.Add(new MultiplayerPlayer(spawnedPlayer.GetComponent<PlayerInputController>(), GetNextNetworkID()));

            GameObject.Destroy(spawnPoint);
        }

        public static void StartServer(int port = 8606)
        {
            GenericSetup();

            SingleplayerServerStarter.Instance.StartServerThenCall(delegate // clone drone requires us to start a bolt server to spawn players and such
            {
                NetworkingCore.StartServer(port);

                CurrentGameData.CurentLevelID = "ModdedMultiplayerTestLevel.json";

                LevelManager.Instance.SpawnCurrentLevel(false).MoveNext();

                SpawnPlayer(new Vector3(0, 10, 0), true);
                
            });
            
        }
        
        public const string TEMP_MAP_NAME = "tempLevel.json";

        public static void StartClient(string ip, int port = 8606)
        {
            GenericSetup();

            SingleplayerServerStarter.Instance.StartServerThenCall(delegate // clone drone requires us to start a bolt server to spawn players and such
            {

                NetworkingCore.StartClient(ip, port, delegate
                {
					ThreadSafeDebug.Log("Connected!");



                });
                
                //LevelManager.Instance.SpawnCurrentLevel(false).MoveNext();

                //SpawnPlayer(new Vector3(0, 10, 0), true);


            });
        }

        public static void GenericSetup()
        {
            CurrentGameData = new GameData()
            {
                HumanFacts = HumanFactsManager.Instance.GetRandomFactSet()
            };

            LevelManager.Instance.CleanUpLevelThisFrame();
            GameFlowManager.Instance.HideTitleScreen(true);
            Accessor.SetPrivateField("_gameMode", GameFlowManager.Instance, Main.MODDED_MULTIPLAYER_TEXT_GAMEMODE);

            CacheManager.Instance.CreateOrClearInstance();
            GameObject.FindObjectOfType<HideIfLevelHidesArena>().gameObject.SetActive(false);

            Accessor.SetPrivateField("_currentLevelHidesTheArena", LevelManager.Instance, true);
        }
        public static short GetNextNetworkID()
        {
            return 0;
        }

    }

    public class MultiplayerPlayer
    {

        public PlayerInputController PlayerInput;
        public short NetworkID;

        public MultiplayerPlayer(PlayerInputController inputController, short networkID)
        {
			PlayerInput=inputController;
            NetworkID=networkID;
        }

    }
}
