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
        public const int SEND_LEVEL_BYTES_PER_MSG = 512;
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
            byte[] buffer = new byte[SEND_LEVEL_BYTES_PER_MSG];
            NetworkingCore.ScheduleForMainThread(delegate { debug.Log(0); });
            socket.Receive(buffer);
            short length = BitConverter.ToInt16(new byte[] { buffer[0], buffer[1] }, 0);
            byte[][] reconstructedFileStep1 = new byte[length][];
            reconstructedFileStep1[BitConverter.ToInt16(new byte[] { buffer[2], buffer[3] }, 0)] = buffer;
            NetworkingCore.ScheduleForMainThread(delegate { debug.Log(1); });
            NetworkingCore.ScheduleForMainThread(delegate { debug.Log("length: " + length); });
            for(int i = 1; i < length; i++)
            {
                socket.Receive(buffer);

                for(int j = 0; j < buffer.Length; j++)
                {
                    Console.Write(j + ": ");
                    Console.Write(buffer[j]);
                    Console.WriteLine();
                }

                short index = BitConverter.ToInt16(new byte[] { buffer[2], buffer[3] }, 0);
                NetworkingCore.ScheduleForMainThread(delegate { debug.Log("index: " + index); });
                reconstructedFileStep1[index] = buffer;
            }
            NetworkingCore.ScheduleForMainThread(delegate { debug.Log(2); });
            int endIndex = -1;
            byte[] array = reconstructedFileStep1[length-1];
            for(int i = 0; i < SEND_LEVEL_BYTES_PER_MSG; i++)
            {
                if(array[i] == 0)
                {
                    endIndex = i;
                    break;
                }
            }
            NetworkingCore.ScheduleForMainThread(delegate { debug.Log(3); });
            int fileLength = reconstructedFileStep1.Length * (SEND_LEVEL_BYTES_PER_MSG-4) - (SEND_LEVEL_BYTES_PER_MSG - endIndex);
            byte[] reconstructedFileStep2 = new byte[fileLength];
            int currrentIndex = 0;
            for(int i = 0; i < reconstructedFileStep1.Length; i++)
            {
                for(int j = 4; j < reconstructedFileStep1[j].Length; j++) // skip the first 4 since they are reserved
                {
                    if(currrentIndex < reconstructedFileStep2.Length)
                    {
                        reconstructedFileStep2[currrentIndex] = reconstructedFileStep1[i][j];
                    }
                    currrentIndex++;
                }
            }
            NetworkingCore.ScheduleForMainThread(delegate { debug.Log(4); });
            string path = Application.persistentDataPath + "/ModdedLevels/" + TEMP_MAP_NAME;
            File.Create(path);
            return path;
        }
        public static void SendMap(Socket socket, string jsonPath)
        {
            byte[] fileBytes = File.ReadAllBytes(jsonPath);
            int bytesLength = fileBytes.Length;

            int msgCount = (byte)Math.Ceiling(bytesLength/(float)(SEND_LEVEL_BYTES_PER_MSG-4));

            if(msgCount > short.MaxValue)
            {
                NetworkingCore.ScheduleForMainThread(delegate { Debug.LogError("level too big :/ length: " + bytesLength); });
                return;
            }


            byte[] buffer = new byte[SEND_LEVEL_BYTES_PER_MSG];
            for(short i = 0; i < msgCount; i++)
            {
                byte[] msgCountBytes = BitConverter.GetBytes((short)msgCount);
                buffer[0] = msgCountBytes[0];
                buffer[1] = msgCountBytes[1];
                byte[] iBytes = BitConverter.GetBytes(i);
                buffer[2] = iBytes[0];
                buffer[3] = iBytes[1];

                for(int j = 4; j < SEND_LEVEL_BYTES_PER_MSG; j++)
                {
                    int offset = i*(SEND_LEVEL_BYTES_PER_MSG-4) + j - 4; // calculates the offset in the fileBytes array, -2 since the 2 first bytes are reserved for total length and current index

                    if(offset >= fileBytes.Length) // if the file has ended we dont want to get an index out of range exception
                    {
                        buffer[j] = 0;
                    }
                    else
                    {
                        buffer[j] = fileBytes[offset];
                    }

                }

                socket.Send(buffer);
            }

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
