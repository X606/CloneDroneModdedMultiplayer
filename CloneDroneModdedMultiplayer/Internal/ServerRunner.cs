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
using CloneDroneModdedMultiplayer.HighLevelNetworking;
using CloneDroneModdedMultiplayer.Internal.Messages;

namespace CloneDroneModdedMultiplayer.Internal
{
    public static class ServerRunner
    {
        public static GameData CurrentGameData;

		public static Dictionary<ushort, MultiplayerPlayer> Players = new Dictionary<ushort, MultiplayerPlayer>();

		public const string TEMP_MAP_NAME = "tempLevel.json";

		public static void SpawnPhysicalPlayer(ushort playerID, Vector3 position, float rotation)
		{
			MultiplayerPlayer player = Players[playerID];

			GameObject spawnPoint = new GameObject("TempSpawnPoint" + playerID);
			spawnPoint.transform.position = position;
			spawnPoint.transform.eulerAngles = new Vector3(0, rotation, 0);

			CharacterModel overrideModel = null;
			Character selectedCharacter = EnemyFactory.Instance.GetEnemyPrefab(player.CharacterModelOverrideType);
			if (selectedCharacter is FirstPersonMover)
				overrideModel = (selectedCharacter as FirstPersonMover).CharacterModelPrefab;

			GameFlowManager.Instance.SpawnPlayer(spawnPoint.transform, true, player.IsLocalPlayer, player.PlayerColor, overrideModel);

			GameObject.Destroy(spawnPoint);
		}

		public static void StartServer(int port = 8606)
        {
            GenericSetup();

            SingleplayerServerStarter.Instance.StartServerThenCall(delegate // clone drone requires us to start a bolt server to spawn players and such
            {
				NetworkingCore.SERVER_OnClientConnected += SERVER_OnClientConnected;

                NetworkingCore.StartServer(port);

				RegisterHandelers();
				
                CurrentGameData.CurentLevelID = "ModdedMultiplayerTestLevel.json";

                LevelManager.Instance.SpawnCurrentLevel(false).MoveNext();

                SpawnPlayer(new Vector3(0, 10, 0), true);
                
            });
            
        }

		static void SERVER_OnClientConnected(ConnectedClient obj) // this will NOT run in the main thread
		{
			ThreadSafeDebug.Log("Client connected from " + obj.TcpConnection.RemoteEndPoint.ToString());

			string levelPath = LevelManager.Instance.GetCurrentLevelDescription().LevelJSONPath;
			byte[] bytes = File.ReadAllBytes(levelPath);
			MapSendingMessge.Send(bytes); // send map to other client


		}

		public static void StartClient(string ip, int port = 8606)
        {
            GenericSetup();

			SingleplayerServerStarter.Instance.StartServerThenCall(delegate // clone drone requires us to start a bolt server to spawn players and such
			{
				NetworkingCore.StartClient(ip, port);
				ThreadSafeDebug.Log("Connected!");

				RegisterHandelers();

				MapSendingMessge.OnMapSpawnedClient += delegate
				{
					SpawnPlayer(new Vector3(0, 10, 0), true);
				};

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
            Accessor.SetPrivateField("_gameMode", GameFlowManager.Instance, Main.MODDED_MULTIPLAYER_TEST_GAMEMODE);

            CacheManager.Instance.CreateOrClearInstance();
            GameObject.FindObjectOfType<HideIfLevelHidesArena>().gameObject.SetActive(false);

            Accessor.SetPrivateField("_currentLevelHidesTheArena", LevelManager.Instance, true);
        }

		public static MapSendingMessge MapSendingMessge;
		public static PlayerConnectedMessage PlayerSpawnMessage;

		public static void RegisterHandelers()
		{
			Mod owner = Main.Instance;
			MapSendingMessge = new MapSendingMessge();
			NetworkManager.AddNetworkMessage(MapSendingMessge, owner);

			PlayerSpawnMessage = new PlayerConnectedMessage();
			NetworkManager.AddNetworkMessage(PlayerSpawnMessage, owner);
		}

        public static ushort GetNextNetworkID()
        {
            return 0;
        }

    }

    public class MultiplayerPlayer
    {
        public ushort PlayerID;
		public readonly ushort ServerOwnerClientNetworkID;
		public bool IsLocalPlayer;
		public EnemyType CharacterModelOverrideType;
		public Color PlayerColor;

		public Dictionary<UpgradeType, int> PlayerUpgrades;

		public MultiplayerPlayer(ushort playerID)
        {
            PlayerID=playerID;
        }

    }
}
