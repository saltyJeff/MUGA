using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MUGA {
	/// <summary>
	/// Keeps tracks of which GameObjects are "owned" by who
	/// </summary>
	public static class Ownerships {
		/// <summary>
		/// Holds the ownerships
		/// </summary>
		public static Dictionary<int, List<GameObject>> ownerships = new Dictionary<int, List<GameObject>>();
		/// <summary>
		/// Registers the ownership of a gameObject by a connection
		/// </summary>
		/// <param name="connectionId">The connection identifier.</param>
		/// <param name="go">The gameObject to associated with the connection id</param>
		public static void RegisterOwnership(int connectionId, GameObject go) {
			List<GameObject> owned;
			bool exists = ownerships.TryGetValue(connectionId, out owned);
			if (owned == null) {
				owned = new List<GameObject>();
			}
			owned.Add(go);
			if (!exists) {
				ownerships[connectionId] = owned;
			}
			if(connectionId == -1) {
				OnOwnership(go);
			}
			Debug.Log("Ownership of " + go.name + " given to " + connectionId);
		}
		/// <summary>
		/// Gets the owned objects of a connection
		/// </summary>
		/// <param name="connectionId">The connection identifier.</param>
		/// <returns></returns>
		public static List<GameObject> GetOwnedObjs(int connectionId) {
			if(ownerships[connectionId] != null) {
				return ownerships[connectionId];
			}
			return new List<GameObject>();
		}
		/// <summary>
		/// Called on the client when it recieves a new ownership
		/// </summary>
		/// <param name="owned">The owned.</param>
		public delegate void OwnershipHandler (GameObject owned);
		public static event OwnershipHandler OnOwnership;
	}
}
