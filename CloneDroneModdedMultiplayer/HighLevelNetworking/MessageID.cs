using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneDroneModdedMultiplayer.HighLevelNetworking
{
	public struct MessageID
	{
		public MessageID(ushort rawValue)
		{
			_value = rawValue;
		}
		public MessageID(byte[] data)
		{
			_value = BitConverter.ToUInt16(data, 0);
		}

		private ushort _value;

		const int BITS_IN_USHORT = sizeof(ushort)*8;

		// MSG_ID_BIT_LENGTH is the amount of bits we want to use of our 16 bits for the msg id, 
		// keep in mind that the rest of the bits are used for the mod id so make sure to leave some free.
		const ushort MSG_ID_BIT_LENGTH = 10;
		const ushort MOD_ID_BIT_LENGTH = BITS_IN_USHORT - MSG_ID_BIT_LENGTH;

		const ushort MSG_ID_MASK = (1<<MSG_ID_BIT_LENGTH)-1;                           // this makes a value that is 0000001111111111 in binary
		const ushort MOD_ID_MASK = ((1<<BITS_IN_USHORT)-1)^((1<<MSG_ID_BIT_LENGTH)-1); // this makes a value that is 1111110000000000 in binary

		public ushort ModID
		{
			get
			{
				ushort value = (ushort)(_value & MOD_ID_MASK); // Masks out the mod id part of the value
				value = (ushort)(value >> MSG_ID_BIT_LENGTH); // Bitshifts the value to the proper offset
				return value;
			}
			set
			{
				if(value >= 1<<MOD_ID_BIT_LENGTH)
					throw new Exception("Value cannot be larger than " + ((1<<MOD_ID_BIT_LENGTH)-1));

				ushort bitshiftedValue = (ushort)(value << MSG_ID_BIT_LENGTH); // bitshifts the value to the proper offset
				_value = (ushort)((_value & MSG_ID_MASK) | bitshiftedValue); // first gets rid of the old value by bitmasking out it, then ors in the new value
			}
		}
		public ushort MsgID
		{
			get
			{
				ushort value = (ushort)(_value & MSG_ID_MASK); // ands out the part of the value we want
				return value;
			}
			set
			{
				if(value >= 1<<MSG_ID_BIT_LENGTH)
					throw new Exception("Value cannot be larger than " + ((1<<MSG_ID_BIT_LENGTH)-1));

				_value = (ushort)((_value & MOD_ID_MASK) | value); // ands out the old value, and ors in the new value in its place
			}
		}
		public ushort RawValue
		{
			get
			{
				return _value;
			}
		}
		public static bool operator ==(MessageID left, MessageID right)
		{
			return left.RawValue == right.RawValue;
		}
		public static bool operator !=(MessageID left, MessageID right)
		{
			return left.RawValue != right.RawValue;
		}
		public override bool Equals(object obj)
		{
			if(!(obj is MessageID))
				return false;

			return this == (MessageID)obj;
		}
		public override int GetHashCode()
		{
			return _value;
		}
	}
}
