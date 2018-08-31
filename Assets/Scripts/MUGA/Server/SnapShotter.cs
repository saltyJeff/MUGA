using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System;

namespace MUGA.Server {
	public class SnapShotter {
		public static LinkedList<NetworkTransform> objectsToWatch = new LinkedList<NetworkTransform>();

		//saves both collider states & gameobject states. Colliders are important for lag compensated physics, saved gameobject states important for lag interpolation
		private LinkedList<TransformProfile>[] savedColliderStates;
		private Dictionary<uint, TransformProfile>[] savedGameObjectStates;
		/*
		 * But wait you (me in 2 days) ask why don't you just take the entries from the dictionary?
		 * Because each GameObject contains more than one collider, and just sending the game object's transform
		 * without the collier is cheaper on bandwidth
		 * 
		*/
		private int snapMemoryCount;
		private long ticksPerSample;
		public int currentSnapshot = -1;

		private long zeroTickTime;

		public SnapShotter(long ticksPerSamp, int snapsToRemember) {
			ticksPerSample = ticksPerSamp;
			snapMemoryCount = snapsToRemember;
			savedColliderStates = new LinkedList<TransformProfile>[snapMemoryCount];
			savedGameObjectStates = new Dictionary<uint, TransformProfile>[snapMemoryCount];
			for(int i = 0; i < savedGameObjectStates.Length; i++) {
				savedGameObjectStates[i] = new Dictionary<uint, TransformProfile>();
			}
		}
		internal void TakeSnapshot() {
			//hacked together queue
			int snapIdx = ++currentSnapshot % snapMemoryCount;

			LinkedList<TransformProfile> profiles = new LinkedList<TransformProfile>();
			savedGameObjectStates[snapIdx].Clear();
			//snapshot all the network transforms
			LinkedListNode<NetworkTransform> node = objectsToWatch.First;
			while(node != null) {
				LinkedListNode<NetworkTransform> next = node.Next;
				NetworkTransform transform = node.Value;
				
				//clear out any "destroyed" nodes
				if (transform == null) {
					objectsToWatch.Remove(node);
				}
				else {
					//snapshot the transform
					savedGameObjectStates
						[snapIdx]
						[transform.gameObjectId] = new TransformProfile(transform.gameObject);
					//iterate through every collider
					foreach (Collider collider in transform.watchedColliders) {
						profiles.AddFirst(TransformProfile.ProfileFromCollider(collider));
					}
				}

				node = next;
			}
			//clear linkedlist in O(1) time while shitting on the garbage collector
			savedColliderStates[snapIdx] = profiles;

			if(currentSnapshot == 0) {
				//figure out what 0 time is
				zeroTickTime = Utils.Timestamp;
			}
		}
		internal Dictionary<uint, TransformProfile> GetGameObjectSnapshot(int snap) {
			return savedGameObjectStates[snap % snapMemoryCount];
		}
		private bool SampleExistsForTime(long snapTime, out int sampleIndex) {
			long delta = (Utils.Timestamp - snapTime) / ticksPerSample;
			sampleIndex = (int)(((snapTime - zeroTickTime) / ticksPerSample) % snapMemoryCount);
			return delta <= savedColliderStates.Length;
		}
		internal LinkedList<TransformProfile> GetColliderSnapshot(long snapTime) {
			int idx;
			if (SampleExistsForTime(snapTime, out idx)) {
				return savedColliderStates[idx];
			}
			throw new InvalidOperationException("nope, attempted to get a collider snapshot from too long ago");
		}
		internal Dictionary<uint, TransformProfile> GetGameObjectSnapshot(long snapTime) {
			int idx;
			if (SampleExistsForTime(snapTime, out idx)) {
				return savedGameObjectStates[idx];
			}
			throw new InvalidOperationException("nope, attempted to get a gameobject snapshot from too long ago");
		}
	}
}
