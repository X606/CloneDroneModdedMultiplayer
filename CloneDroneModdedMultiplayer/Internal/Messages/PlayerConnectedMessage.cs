using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloneDroneModdedMultiplayer.HighLevelNetworking;
using UnityEngine;

namespace CloneDroneModdedMultiplayer.Internal.Messages
{
	public class PlayerConnectedMessage : NetworkMessageBase
	{
		public override string Name => nameof(PlayerConnectedMessage);
		public override MessageChannel Channel => MessageChannel.Safe;
		protected override ushort MessageID => 1;

		protected override void OnPackageReceivedClient(byte[] package)
		{
			ThreadSafeDebug.Log("2");
			CreatedPlayerInfo playerInfo = new CreatedPlayerInfo();
			playerInfo.DeserializeInto(package);

			ServerRunner.Players.Add(playerInfo.PlayerID, new MultiplayerPlayer(playerInfo.PlayerID)
			{
				CharacterModelOverrideType = playerInfo.CharacterModelOverrideType,
				PlayerColor = playerInfo.PlayerColor,
				PlayerUpgrades = playerInfo.PlayerUpgrades
			});

		}

		public void Send(CreatedPlayerInfo playerInfo)
		{
			byte[] data = playerInfo.SerializeToBytes();
			Send(data);
		}
		public void SendTo(CreatedPlayerInfo playerInfo, ushort reciver)
		{
			byte[] data = playerInfo.SerializeToBytes();
			SendTo(data, reciver);
		}

		public class CreatedPlayerInfo : IByteSerializable<CreatedPlayerInfo>
		{
			public int GetSize()
			{
				return sizeof(ushort) + sizeof(float)*4 + sizeof(bool) + sizeof(int)*2 + PlayerUpgrades.Count*sizeof(int)*2; // MAKE SURE YOU UPDATE THIS IF FIELDS ARE CHANGED
			}

			public byte[] SerializeToBytes()
			{
				byte[] buffer = new byte[GetSize()];

				int fileOffset = 0;

				Buffer.BlockCopy(BitConverter.GetBytes(PlayerID), 0, buffer, fileOffset, sizeof(ushort));
				fileOffset += sizeof(ushort);

				Buffer.BlockCopy(BitConverter.GetBytes(PlayerColor.r), 0, buffer, fileOffset, sizeof(float));
				fileOffset += sizeof(float);
				Buffer.BlockCopy(BitConverter.GetBytes(PlayerColor.g), 0, buffer, fileOffset, sizeof(float));
				fileOffset += sizeof(float);
				Buffer.BlockCopy(BitConverter.GetBytes(PlayerColor.b), 0, buffer, fileOffset, sizeof(float));
				fileOffset += sizeof(float);
				Buffer.BlockCopy(BitConverter.GetBytes(PlayerColor.a), 0, buffer, fileOffset, sizeof(float));
				fileOffset += sizeof(float);

				Buffer.BlockCopy(BitConverter.GetBytes((int)CharacterModelOverrideType), 0, buffer, fileOffset, sizeof(float));
				fileOffset += sizeof(int);

				Buffer.BlockCopy(BitConverter.GetBytes(PlayerUpgrades.Count), 0, buffer, fileOffset, sizeof(int));
				fileOffset += sizeof(int);

				foreach(KeyValuePair<UpgradeType, int> keyPair in PlayerUpgrades)
				{
					Buffer.BlockCopy(BitConverter.GetBytes((int)keyPair.Key), 0, buffer, fileOffset, sizeof(int));
					fileOffset += sizeof(int);

					Buffer.BlockCopy(BitConverter.GetBytes(keyPair.Value), 0, buffer, fileOffset, sizeof(int));
					fileOffset += sizeof(int);
				}

				return buffer;
			}

			public void DeserializeInto(byte[] data)
			{
				int fileOffset = 0;
				PlayerID = BitConverter.ToUInt16(data, fileOffset);
				fileOffset += sizeof(ushort);

				Color color = new Color();
				color.r = BitConverter.ToSingle(data, fileOffset);
				fileOffset += sizeof(float);
				color.g = BitConverter.ToSingle(data, fileOffset);
				fileOffset += sizeof(float);
				color.b = BitConverter.ToSingle(data, fileOffset);
				fileOffset += sizeof(float);
				color.a = BitConverter.ToSingle(data, fileOffset);
				fileOffset += sizeof(float);
				PlayerColor = color;

				CharacterModelOverrideType = (EnemyType)BitConverter.ToInt32(data, fileOffset);
				fileOffset += sizeof(int);

				int length = BitConverter.ToInt32(data, fileOffset);
				fileOffset += sizeof(int);

				Dictionary<UpgradeType, int> playerUpgrades = new Dictionary<UpgradeType, int>();
				for(int i = 0; i < length; i++)
				{
					UpgradeType upgradeType = (UpgradeType)BitConverter.ToInt32(data, fileOffset);
					fileOffset += sizeof(int);

					int level = BitConverter.ToInt32(data, fileOffset);
					fileOffset += sizeof(int);
					playerUpgrades.Add(upgradeType, level);
				}
				PlayerUpgrades = playerUpgrades;
			}

			public ushort PlayerID;
			public Color PlayerColor;
			public EnemyType CharacterModelOverrideType;

			public Dictionary<UpgradeType, int> PlayerUpgrades;
		}
	}
}
