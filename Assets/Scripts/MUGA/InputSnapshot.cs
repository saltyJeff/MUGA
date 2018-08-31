using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MUGA {
	/// <summary>
	/// Represents a snapshot of the user's input to send to the server
	/// </summary>
	[MessagePackObject(keyAsPropertyName: true)]
	public class InputSnapshot {
		/// <summary>
		/// The axes
		/// </summary>
		public Dictionary<string, float> axes = new Dictionary<string, float>();
		/// <summary>
		/// The buttons
		/// </summary>
		public Dictionary<string, bool> buttons = new Dictionary<string, bool>();
		/// <summary>
		/// The time this snapshot represents (in ticks)
		/// </summary>
		public long timeSent;

		//no touchy
		[IgnoreMember]
		protected NetworkInstanceId Sender;

		public InputSnapshot() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InputSnapshot"/> class.
		/// </summary>
		/// <param name="time">The time that the snapshot represents</param>
		public InputSnapshot(long time) {
			timeSent = time;
		}
		/// <summary>
		/// Gets an axis by name
		/// </summary>
		/// <param name="name">The name of the axis</param>
		/// <returns>the value of the axis or 0 if not represented</returns>
		public float GetAxis(string name) {
			float value;
			axes.TryGetValue(name, out value);
			return value;
		}
		/// <summary>
		/// Gets a button by name
		/// </summary>
		/// <param name="name">The name of the button</param>
		/// <returns>the value of the button or 0 if not represented</returns>
		public bool GetButton(string name) {
			bool value;
			buttons.TryGetValue(name, out value);
			return value;
		}
		/// <summary>
		/// Sets an axis by name
		/// </summary>
		/// <param name="name">The name of the axis</param>
		/// <param name="val">The value of the axis to set</param>
		public void SetAxis(string name, float val) {
			axes[name] = val;
		}
		/// <summary>
		/// Sets a button by name
		/// </summary>
		/// <param name="name">The name of the button</param>
		/// <param name="val">The value of the button to set</param>
		public void SetButton(string name, bool val) {
			buttons[name] = val;
		}
		/// <summary>
		/// Sets a button by name to True
		/// </summary>
		/// <param name="name">The name of the button</param>
		public void SetButton(string name) {
			SetButton(name, true);
		}
		/// <summary>
		/// Serializes the input snapshot into a byte[]
		/// </summary>
		/// <returns>byte[] representing the snapshot</returns>
		public byte[] ToBytes() {
			return MessagePackSerializer.Serialize(this);
		}
		/// <summary>
		/// Deserializes an input snapshot from a byte[]
		/// </summary>
		/// <param name="b">The byte[]</param>
		/// <returns>The represented snapshot</returns>
		public static InputSnapshot FromBytes(byte[] b) {
			return MessagePackSerializer.Deserialize<InputSnapshot>(b);
		}
	}
}

