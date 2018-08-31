using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace MUGA {
	/// <summary>
	/// Represents a state update for the entire game
	/// </summary>
	[MessagePackObject(keyAsPropertyName: true)]
	public class StateUpdate {
		public const string ABSOLUTE_SNAPSHOT = "ABS";
		public const string DELTA_SNAPSHOT = "DELT";

		/// <summary>
		/// The timestamp that the state update represents
		/// </summary>
		public long timestamp;
		/// <summary>
		/// The game object profiles mapped to their IDs
		/// </summary>
		public Dictionary<uint, TransformProfile> gameObjectProfiles;
		/// <summary>
		/// Either an  absolute snapshot or a delta snapshot
		/// </summary>
		public string type;

		public StateUpdate() { }
		public StateUpdate(long timestamp, Dictionary<uint, TransformProfile> gameObjectProfiles, bool absoluteNotDelta) {
			this.timestamp = timestamp;
			this.gameObjectProfiles = gameObjectProfiles;
			this.type = absoluteNotDelta ? ABSOLUTE_SNAPSHOT : DELTA_SNAPSHOT;
		}
		/// <summary>
		/// Serializes the state update to bytes
		/// </summary>
		/// <returns>The serialized byte[]</returns>
		public byte[] ToBytes() {
			return MessagePackSerializer.Serialize(this);
		}
		/// <summary>
		/// Parses a state update from a byte[]
		/// </summary>
		/// <param name="b">byte[] to parse</param>
		/// <returns>The represented StateUpdate</returns>
		public static StateUpdate FromBytes(byte[] b) {
			return MessagePackSerializer.Deserialize<StateUpdate>(b);
		}
	}
}
