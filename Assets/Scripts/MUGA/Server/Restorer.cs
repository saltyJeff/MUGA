using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MUGA.Server {
	/// <summary>
	/// A one-time class used to restore the state of the game back to a previous time. It should
	/// only be used within a using() block
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class Restorer: IDisposable {
		//in case we can ever use multiple threads we'll encapsulate everything here
		private ComponentPool<BoxCollider> boxPool;
		private ComponentPool<CapsuleCollider> capsulePool;
		private ComponentPool<SphereCollider> spherePool;
		private LinkedList<TransformProfile> profiles;
		internal Restorer(ComponentPool<BoxCollider> bP, ComponentPool<CapsuleCollider> cP, ComponentPool<SphereCollider> sP, LinkedList<TransformProfile> prof) {
			boxPool = bP;
			capsulePool = cP;
			spherePool = sP;

			profiles = prof;

			//disable all the colliders for the objects in realtime
			foreach(NetworkTransform trans in SnapShotter.objectsToWatch) {
				trans.SetCollidersEnabled(false);
			}
			foreach (TransformProfile profile in profiles) {
				RestoreFromProfile(profile);
			}
		}
		private void RestoreFromProfile(TransformProfile profile) {
			if(profile == null) {
				throw new InvalidOperationException();
			}
			CapsuleColliderProfile maybeCapsule = profile as CapsuleColliderProfile;
			if (maybeCapsule != null) {
				CapsuleCollider c = capsulePool.TakeComponent();
				maybeCapsule.RestoreSelfToCollider(c);
				return;
			}
			BoxColliderProfile maybeBox = profile as BoxColliderProfile;
			if (maybeBox != null) {
				BoxCollider c = boxPool.TakeComponent();
				maybeBox.RestoreSelfToCollider(c);
				return;
			}
			SphereColliderProfile maybeSphere = profile as SphereColliderProfile;
			if (profile != null) {
				SphereCollider c = spherePool.TakeComponent();
				maybeSphere.RestoreSelfToCollider(c);
				return;
			}
			throw new InvalidOperationException("howdafuq do we have an invalid collider profile? got: "+profile.GetType().ToString()+" maybeCapsule: "+maybeCapsule+" maybeBox: "+maybeBox);
		}

		public void Dispose() {
			boxPool.ClearPool();
			capsulePool.ClearPool();
			spherePool.ClearPool();
			//restore all the realtime colliders
			foreach (NetworkTransform trans in SnapShotter.objectsToWatch) {
				trans.SetCollidersEnabled(true);
			}
		}
	}
}
