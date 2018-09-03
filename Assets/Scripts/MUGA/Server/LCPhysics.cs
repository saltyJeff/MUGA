using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MUGA.Server {
	/// <summary>
	/// Performs lag-compensated physics
	/// </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public class LCPhysics: MonoBehaviour {
		/// <summary>
		/// The physics samples per second
		/// </summary>
		public int samplesPerSecond = 30;
		/// <summary>
		/// How many previous samples to remember (in seconds)
		/// </summary>
		public float maxAcceptableLag = 0.75f;
		private long ticksPerSample; //cache value
		private SnapShotter snapShotter;
		private int snapsRemembered;

		/// <summary>
		/// How many starting box colliders should be in the pool
		/// </summary>
		public int boxPoolStartAmount = 10;
		/// <summary>
		/// How many starting capsule colliders should be in the pool
		/// </summary>
		public int capsulePoolStartAmount = 10;
		/// <summary>
		/// How many starting sphere colliders should be in the pool
		/// </summary>
		public int spherePoolStartAmount = 10;
		private ComponentPool<BoxCollider> boxPool;
		private ComponentPool<CapsuleCollider> capsulePool;
		private ComponentPool<SphereCollider> spherePool;

		//super ugly singleton design (i know)
		private static LCPhysics inst;

		//events
		/// <summary>
		/// A delegate called every time a physics sample is taken
		/// </summary>
		/// <param name="timeFrame">The time frame.</param>
		/// <param name="snapshot">The snapshot.</param>
		public delegate void PhysicsTickHandler(long timeFrame, Dictionary<uint, TransformProfile> snapshot);
		public event PhysicsTickHandler OnPhysicsTick;

		//coroutine stuff
		private Coroutine physicsUpdate;

		// Use this for initialization
		void Awake() {
			if(inst != null) {
				throw new InvalidOperationException("There must be more than 1 LC physics class in the scene. There can only be 1");
			}
			inst = this;

			ticksPerSample = Utils.TICKS_PER_SEC / samplesPerSecond;
			Debug.Log("ticks per sample: "+ticksPerSample);
			snapsRemembered = Mathf.CeilToInt(samplesPerSecond * maxAcceptableLag);

		}

		/// <summary>
		/// Begins the physics sampler.
		/// </summary>
		public void BeginPhysics() {
			if (physicsUpdate != null) {
				Debug.LogException(new InvalidOperationException("Attempted to start physics when it is already running. Ignoring request"));
				return;
			}
			snapShotter = new SnapShotter(ticksPerSample, snapsRemembered);
			boxPool = new ComponentPool<BoxCollider>(boxPoolStartAmount);
			capsulePool = new ComponentPool<CapsuleCollider>(capsulePoolStartAmount);
			spherePool = new ComponentPool<SphereCollider>(spherePoolStartAmount);
			physicsUpdate = StartCoroutine(ExecPhysics());
		}
		/// <summary>
		/// Ends the physics sampler.
		/// </summary>
		public void EndPhysics() {
			if (physicsUpdate == null) {
				Debug.LogException(new InvalidOperationException("Attempted to stop physics when it is already stopped. Ignoring request"));
				return;
			}
			StopCoroutine(physicsUpdate);
			physicsUpdate = null;
		}
		//loop to take snapshots
		private IEnumerator ExecPhysics() {
			while (true) {
				long startTime = Utils.Timestamp;
				snapShotter.TakeSnapshot();

				OnPhysicsTick(startTime, snapShotter.GetGameObjectSnapshot(snapShotter.currentSnapshot));
				long endTime = Utils.Timestamp;
				float secsToSpare = ((float)ticksPerSample - (endTime - startTime)) / Utils.TICKS_PER_SEC;
				if (secsToSpare <= 0) {
					Debug.LogWarning("tick " + snapShotter.currentSnapshot + " took too long by " + (secsToSpare * -1000) + " ms");
				}
				yield return new WaitForSecondsRealtime(secsToSpare);
			}
		}

		/// <summary>
		/// Adds a NetworkTransform to the list of objects to take physics samples of
		/// </summary>
		/// <param name="nt">The network transform</param>
		public static void WatchObject(NetworkTransform nt) {
			SnapShotter.objectsToWatch.AddLast(nt);
		}
		/// <summary>
		/// Gets a restorer for a certain time in the past.
		/// </summary>
		/// <param name="restoreTime">The time of the state to restore</param>
		/// <returns>A restorer representing a time in the past</returns>
		public static Restorer GetRestorer(long restoreTime) {
			return new Restorer(inst.boxPool, inst.capsulePool, inst.spherePool, inst.snapShotter.GetColliderSnapshot(restoreTime));
		}
		/// <summary>
		/// Gets the ticks per sample.
		/// </summary>
		/// <returns>The ticks per sample</returns>
		public static float GetTicksPerSample() {
			return inst.ticksPerSample;
		}
	}
}

