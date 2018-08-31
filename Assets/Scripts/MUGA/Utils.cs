using MessagePack;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace MUGA {
	/// <summary>
	/// Time utilities for absolute system time instead of Unity's time system
	/// </summary>
	public static class Utils {
		/// <summary>
		/// The ticks per sec
		/// </summary>
		public const long TICKS_PER_SEC = 10000000;
		/// <summary>
		/// Gets the timestamp in C# Ticks.
		/// </summary>
		/// <value>
		/// The timestamp.
		/// </value>
		public static long Timestamp {
			get {
				return System.DateTimeOffset.UtcNow.Ticks;
			}
		}
	}
}
