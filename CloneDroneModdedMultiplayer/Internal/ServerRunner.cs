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
		public static ushort? LocalPlayerID = null;

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

			GameFlowManager.Instance.SpawnPlayer(spawnPoint.transform, true, LocalPlayerID.HasValue && playerID == LocalPlayerID.Value, player.PlayerColor, overrideModel);

			GameObject.Destroy(spawnPoint);
		}

		public static void StartServer(int port = 8606)
        {
            GenericSetup();

            SingleplayerServerStarter.Instance.StartServerThenCall(delegate // clone drone requires us to start a bolt server to spawn players and such
            {
				NetworkingCore.SERVER_OnClientConnected += SERVER_OnClientConnected;

                NetworkingCore.StartServer(port);

				RegisterNetworkMessageHandelers();
				
                CurrentGameData.CurentLevelID = "ModdedMultiplayerTestLevel.json";

                LevelManager.Instance.SpawnCurrentLevel(false).MoveNext();
                
            });
            
        }

		static void SERVER_OnClientConnected(ConnectedClient client) // this will NOT run in the main thread
		{
			ThreadSafeDebug.Log("Client connected from " + client.TcpConnection.RemoteEndPoint.ToString());
			
			string levelPath = LevelManager.Instance.GetCurrentLevelDescription().LevelJSONPath;
			byte[] bytes = File.ReadAllBytes(levelPath);
			MapSendingMessge.SendTo(bytes, client.ClientNetworkID); // send map to other client

			ushort playerID = GetNextPlayerID();

			SetLocalPlayerMessage.SendTo(playerID, client.ClientNetworkID);

			var createdPlayerInfo = new PlayerConnectedMessage.CreatedPlayerInfo()
			{
				CharacterModelOverrideType = EnemyType.None,
				PlayerColor = HumanFactsManager.Instance.FavouriteColors[HumanFactsManager.Instance.GetRandomColorIndex()].ColorValue,
				PlayerID = playerID,
				PlayerUpgrades = new Dictionary<UpgradeType, int>()
			};
			PlayerConnectMessage.Send(createdPlayerInfo);

			var spawnPlayerMessage = new SpawnPlayerMessage.SpawnedPlayerInfo()
			{
				PlayerID = playerID,
				Position = new Vector3(0f, 10f, 0f),
				Rotation = 0f
			};
			SpawnPlayerMessage.Send(spawnPlayerMessage);
		}

		public static void StartClient(string ip, int port = 8606)
        {
            GenericSetup();

			SingleplayerServerStarter.Instance.StartServerThenCall(delegate // clone drone requires us to start a bolt server to spawn players and such
			{
				NetworkingCore.StartClient(ip, port);
				ThreadSafeDebug.Log("Connected!");

				RegisterNetworkMessageHandelers();

				MapSendingMessge.OnMapSpawnedClient += delegate
				{
					
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
		public static PlayerConnectedMessage PlayerConnectMessage;
		public static SpawnPlayerMessage SpawnPlayerMessage;
		public static SetLocalPlayerMessage SetLocalPlayerMessage;
		public static DebugMessage DebugMessage;

		public static void RegisterNetworkMessageHandelers()
		{
			Mod owner = Main.Instance;
			MapSendingMessge = new MapSendingMessge();
			NetworkManager.AddNetworkMessage(MapSendingMessge, owner);

			PlayerConnectMessage = new PlayerConnectedMessage();
			NetworkManager.AddNetworkMessage(PlayerConnectMessage, owner);

			SpawnPlayerMessage = new SpawnPlayerMessage();
			NetworkManager.AddNetworkMessage(SpawnPlayerMessage, owner);

			SetLocalPlayerMessage = new SetLocalPlayerMessage();
			NetworkManager.AddNetworkMessage(SetLocalPlayerMessage, owner);

			DebugMessage = new DebugMessage();
			NetworkManager.AddNetworkMessage(DebugMessage, owner);
		}

		static volatile ushort currentPlayerID = 0;
        public static ushort GetNextPlayerID()
        {
			ushort playerID = currentPlayerID;
			currentPlayerID++;
			return playerID;
        }

    }

    public class MultiplayerPlayer
    {
        public ushort PlayerID;
		public readonly ushort ServerOwnerClientNetworkID;
		public EnemyType CharacterModelOverrideType;
		public Color PlayerColor;

		public Dictionary<UpgradeType, int> PlayerUpgrades;

		public MultiplayerPlayer(ushort playerID)
        {
            PlayerID=playerID;
        }

    }
}
