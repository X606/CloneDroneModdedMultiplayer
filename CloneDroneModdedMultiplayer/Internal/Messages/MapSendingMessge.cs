﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloneDroneModdedMultiplayer.HighLevelNetworking;
using UnityEngine;
using System.IO;
using ModLibrary;

namespace CloneDroneModdedMultiplayer.Internal.Messages
{
	public class MapSendingMessge : NetworkMessageBase
	{
		protected override ushort MessageID => 0;
		public override MessageChannel Channel => MessageChannel.Safe;
		public override string Name => "Map sending message";

		public static event Action OnMapSpawnedClient;

		protected override void OnPackageReceivedClient(byte[] package)
		{
			string path = Application.persistentDataPath + "/ModdedLevels/" + ServerRunner.TEMP_MAP_NAME;
			File.WriteAllBytes(path, package);

			ServerRunner.CurrentGameData.CurentLevelID = ServerRunner.TEMP_MAP_NAME;
			LevelManager.Instance.SpawnCurrentLevel(false).MoveNext();
			OnMapSpawnedClient();

		}

	}
}
