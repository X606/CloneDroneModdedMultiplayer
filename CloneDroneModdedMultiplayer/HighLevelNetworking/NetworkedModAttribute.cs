using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneDroneModdedMultiplayer.HighLevelNetworking
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class NetworkedModAttribute : Attribute
	{
	}
}
