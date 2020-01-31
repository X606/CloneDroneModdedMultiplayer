using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloneDroneModdedMultiplayer.HighLevelNetworking;

namespace CloneDroneModdedMultiplayer.Internal.Messages
{
	public class SetLocalPlayerMessage : NetworkMessageBase
	{
		public override string Name => nameof(SetLocalPlayerMessage);
		public override MessageChannel Channel => MessageChannel.Safe;
		protected override ushort MessageID => 3;

		protected override void OnPackageReceivedClient(byte[] package)
		{
			ThreadSafeDebug.Log("1");
			ServerRunner.LocalPlayerID = BitConverter.ToUInt16(package, 0);
		}

		public void SendTo(ushort playerID, ushort reciver)
		{
			SendTo(BitConverter.GetBytes(playerID), reciver);
		}
	}
}
