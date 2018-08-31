using MUGA.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MUGA.Server {
	/// <summary>
	/// A custom Network Transform implementation for streaming state updates (disable if on client)
	/// </summary>
	[RequireComponent(typeof(NetworkIdentity))]
	public class NetworkTransform: MonoBehaviour {
		/// <summary>
		/// A list of colliders that are "watched" by the lag compensation system for this GameObject
		/// </summary>
		protected internal List<Collider> watchedColliders = new List<Collider>();
		/// <summary>
		/// If set to true the NetworkTransform automatically initializes itself by watching all the colliders of the GameObject and
		/// adding itself to the watched NetworkTransform list of LC Physics
		/// </summary>
		public bool easyInit = true;
		public uint gameObjectId = 69;

		/// <summary>
		/// If set to true, the lag compensator will ignore the transform of this GameObject
		/// </summary>
		public bool skipWatch = false;

		private void Start() {
			if(MUGAClient.isClient) {
				SetCollidersEnabled(GetComponent<InputPredictionInterpolator>() != null);
				enabled = false;
				return;
			}
			gameObjectId = GetComponent<NetworkIdentity>().netId.Value;

			if (easyInit) {
				LCPhysics.WatchObject(this);
				AutoAddCollidersToWatch();
			}
			
		}

		/// <summary>
		/// Automatics watches all the colliders attached to the GameObject
		/// </summary>
		protected virtual void AutoAddCollidersToWatch () {
			watchedColliders.Clear();
			watchedColliders = new List<Collider>(GetComponentsInChildren<Collider>());
		}

		internal void SetCollidersEnabled(bool en) {
			foreach(Collider c in watchedColliders) {
				c.enabled = en;
			}
		}
	}
}
