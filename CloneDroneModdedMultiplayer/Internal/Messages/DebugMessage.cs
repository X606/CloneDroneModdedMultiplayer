using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloneDroneModdedMultiplayer.HighLevelNetworking;

namespace CloneDroneModdedMultiplayer.Internal.Messages
{
	public class DebugMessage : NetworkMessageBase
	{
		public override string Name => nameof(DebugMessage);
		public override MessageChannel Channel => MessageChannel.Safe;
		protected override ushort MessageID => 4;

		protected override void OnPackageReceivedClient(byte[] package)
		{
			ThreadSafeDebug.Log("Got debug message!", UnityEngine.Color.blue);
		}
	}
}
