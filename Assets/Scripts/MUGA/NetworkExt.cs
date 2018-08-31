using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUGA {
	/// <summary>
	/// Holds consts for message types
	/// </summary>
	public static class MsgTypeExt {
		/// <summary>
		/// Message code for recieving a state update
		/// </summary>
		public const short STATE_UPDATE = 420;
		/// <summary>
		/// Message code for a user input
		/// </summary>
		public const short USER_INPUT = 1337;
		/// <summary>
		/// Message code for a notice of ownership of a GameObject
		/// </summary>
		public const short OWNERSHIP = 01134;
	}
	public static class DefaultChannelExt {
		public static int DEFAULT_FRAG_UNRELIABLE = 2;
	}
}
