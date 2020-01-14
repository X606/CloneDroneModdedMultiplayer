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
            Players.Add(new MultiplayerPlayer(spawnedPlayer, GetNextNetworkID()));

            GameObject.Destroy(spawnPoint);
        }

        public static void StartServer(int port = 606)
        {
            Setup();

            SingleplayerServerStarter.Instance.StartServerThenCall(delegate // clone drone requires us to start a bolt server to spawn players and such
            {
                NetworkingCore.StartServer(port, OnClientConnected);

                CurrentGameData.CurentLevelID = "ModdedMultiplayerTestLevel.json";


                LevelManager.Instance.SpawnCurrentLevel(false).MoveNext();

                SpawnPlayer(new Vector3(0, 10, 0), true);
                
            });
            
        }
        
        public const string TEMP_MAP_NAME = "tempLevel.json";

        public static void StartClient(string ip, int port = 606)
        {
            Setup();

            SingleplayerServerStarter.Instance.StartServerThenCall(delegate // clone drone requires us to start a bolt server to spawn players and such
            {

                NetworkingCore.StartClient(ip, port, delegate
                {
                    NetworkingCore.ScheduleForMainThread(delegate
                    {
                        debug.Log("connected!");
                    });

                    ReciveMap(NetworkingCore.CLIENT_ServerConnection);

                    NetworkingCore.ScheduleForMainThread(delegate { 
                        CurrentGameData.CurentLevelID = TEMP_MAP_NAME;

                        LevelManager.Instance.SpawnCurrentLevel(false).MoveNext();

                        SpawnPlayer(new Vector3(0, 10, 0), true);
                    });

                });
                
                //LevelManager.Instance.SpawnCurrentLevel(false).MoveNext();

                //SpawnPlayer(new Vector3(0, 10, 0), true);


            });
        }
        
        public static void OnClientConnected(Socket socket)
        {
            NetworkingCore.ScheduleForMainThread(delegate
            {
                debug.Log("client connected!");
            });

            string jsonPath = LevelManager.Instance.GetCurrentLevelDescription().LevelJSONPath;

            SendMap(socket, jsonPath);
        }

        public static void Setup()
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

        public static string ReciveMap(Socket socket)
        {
            byte[] lengthBytes = new byte[sizeof(int)];
            socket.Receive(lengthBytes);
            socket.Send(lengthBytes);
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] buffer = new byte[length];
            socket.Receive(buffer);

            string path = Application.persistentDataPath + "/ModdedLevels/"+TEMP_MAP_NAME;
            File.WriteAllBytes(path, buffer);
            return path;
        }
        public static void SendMap(Socket socket, string jsonPath)
        {
            byte[] fileBytes = File.ReadAllBytes(jsonPath);

            byte[] lengthAsBytes = BitConverter.GetBytes(fileBytes.Length);

            socket.Send(lengthAsBytes);
            byte[] buffer = new byte[sizeof(int)];
            socket.Receive(buffer);
            if(buffer != lengthAsBytes)
                throw new Exception("Something went wrong");

            socket.Send(fileBytes);


        }
    }

    public class MultiplayerPlayer
    {

        public FirstPersonMover PhysicalPlayer;
        public short NetworkID;

        public MultiplayerPlayer(FirstPersonMover physicalPlayer, short networkID)
        {
            PhysicalPlayer=physicalPlayer;
            NetworkID=networkID;
        }

    }
}
