using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLibrary;
using CloneDroneModdedMultiplayer.HighLevelNetworking;

namespace CloneDroneModdedMultiplayer
{
	public static class Utils
	{
		public static bool IsNetworkedMod(this Mod mod)
		{
			return Attribute.IsDefined(mod.GetType(), typeof(NetworkedModAttribute));
		}

	}
}
