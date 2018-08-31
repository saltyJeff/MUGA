using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MUGA {
	/// <summary>
	/// Consumes input both on the server and the client
	/// </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public abstract class InputConsumer: MonoBehaviour {
		public static InputConsumer inst;

		private void Awake() {
			inst = this;
		}
		// connectionId of -1 means executing locally
		public abstract void ConsumeInput(int connectionId, InputSnapshot input);
	}
}
