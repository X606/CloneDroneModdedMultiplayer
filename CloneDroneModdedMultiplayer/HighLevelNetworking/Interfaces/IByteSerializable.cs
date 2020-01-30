using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneDroneModdedMultiplayer.HighLevelNetworking
{
	public interface IByteSerializable<T>
	{
		int GetSize();

		byte[] SerializeToBytes();
		void DeserializeInto(byte[] data);
	}
}
