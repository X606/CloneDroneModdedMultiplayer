using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloneDroneModdedMultiplayer.HighLevelNetworking;
using UnityEngine;

namespace CloneDroneModdedMultiplayer.Internal.Messages
{
	public class SpawnPlayerMessage : NetworkMessageBase
	{
		public override string Name => nameof(SpawnPlayerMessage);
		public override MessageChannel Channel => MessageChannel.Safe;
		protected override ushort MessageID => 2;
		
		public void Send(SpawnedPlayerInfo spawnedPlayerInfo)
		{
			Send(spawnedPlayerInfo.SerializeToBytes());
		}
		public void SendTo(SpawnedPlayerInfo spawnedPlayerInfo, ushort reciver)
		{
			SendTo(spawnedPlayerInfo.SerializeToBytes(), reciver);
		}
		protected override void OnPackageReceivedClient(byte[] package)
		{
			SpawnedPlayerInfo playerInfo = new SpawnedPlayerInfo();
			playerInfo.DeserializeInto(package);

			ServerRunner.SpawnPhysicalPlayer(playerInfo.PlayerID, playerInfo.Position, playerInfo.Rotation);

		}

		public class SpawnedPlayerInfo : IByteSerializable<SpawnedPlayerInfo>
		{
			public int GetSize()
			{
				return sizeof(ushort) + sizeof(float)*4;
			}

			public byte[] SerializeToBytes()
			{
				byte[] buffer = new byte[GetSize()];

				int fileOffset = 0;

				Buffer.BlockCopy(BitConverter.GetBytes(PlayerID), 0, buffer, fileOffset, sizeof(ushort));
				fileOffset += sizeof(ushort);

				Buffer.BlockCopy(BitConverter.GetBytes(Position.x), 0, buffer, fileOffset, sizeof(float));
				fileOffset += sizeof(float);
				Buffer.BlockCopy(BitConverter.GetBytes(Position.y), 0, buffer, fileOffset, sizeof(float));
				fileOffset += sizeof(float);
				Buffer.BlockCopy(BitConverter.GetBytes(Position.z), 0, buffer, fileOffset, sizeof(float));
				fileOffset += sizeof(float);

				Buffer.BlockCopy(BitConverter.GetBytes(Rotation), 0, buffer, fileOffset, sizeof(float));
				fileOffset += sizeof(float);

				return buffer;
			}

			public void DeserializeInto(byte[] data)
			{
				int fileOffset = 0;
				PlayerID = BitConverter.ToUInt16(data, fileOffset);
				fileOffset += sizeof(ushort);

				Position = new Vector3();
				Position.x = BitConverter.ToSingle(data, fileOffset);
				fileOffset += sizeof(float);
				Position.y = BitConverter.ToSingle(data, fileOffset);
				fileOffset += sizeof(float);
				Position.z = BitConverter.ToSingle(data, fileOffset);
				fileOffset += sizeof(float);

				Rotation = BitConverter.ToSingle(data, fileOffset);
				fileOffset += sizeof(float);

			}

			public ushort PlayerID;
			public Vector3 Position;
			public float Rotation;
		}

	}
}
