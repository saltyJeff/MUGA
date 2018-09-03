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
		/// <summary>
		/// Consumes input snapshots on the client and the server
		/// </summary>
		/// <param name="connectionId">The connection identifier. -1 means executing locally</param>
		/// <param name="input">The input snapshot</param>
		/// <param name="firstExec">if set to <c>true</c> [the client is executing this for the first time].</param>
		public abstract void ConsumeInput(int connectionId, InputSnapshot input, bool firstExec);
	}
}
