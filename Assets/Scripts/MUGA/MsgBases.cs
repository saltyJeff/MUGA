using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace MUGA {
	//stupid unity can't serialize a byte[] directly
	/// <summary>
	/// A MessageBase wrapper for a byte[]
	/// </summary>
	/// <seealso cref="UnityEngine.Networking.MessageBase" />
	class ByteMsgBase : MessageBase {
		public byte[] payload;
		public ByteMsgBase () {}
		public ByteMsgBase(byte[] b) {
			payload = b;
		}
	}
	/// <summary>
	/// A MessageBase wrapper for a uint
	/// </summary>
	/// <seealso cref="UnityEngine.Networking.MessageBase" />
	class UintMsgBase : MessageBase {
		public uint val;
		public UintMsgBase() { }
		public UintMsgBase(uint v) {
			val = v;
		}
	}
}
