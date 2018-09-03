using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MUGA.Server {
	/// <summary>
	/// Server-side script for the authoritative server (disable if running on a client)
	/// </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public class MUGAServer: MonoBehaviour {
		//public use for the scripts
		public static bool isServer {
			get {
				return inst.enabled;
			}
			set {
				//thowaway
			}
		}

		public static MUGAServer inst;

		private Dictionary<uint, TransformProfile> lastSnapshot;
		private InputHandler inputHandler;
		private LCPhysics physics;

		private void Awake() {
			if(inst == null) {
				inst = this;
			}
			else {
				throw new InvalidOperationException("There must be more than 1 MUGAServer class in the scene. There can only be 1");
			}
		}

		void Start () {
			physics = GetComponent<LCPhysics>();
			physics.OnPhysicsTick += OnPhysicsUpdate;
			physics.BeginPhysics();

			inputHandler = new InputHandler(InputConsumer.inst);
			NetworkServer.RegisterHandler(MsgTypeExt.USER_INPUT, inputHandler.HandleInput);
		}
		private void OnDisable() {
			physics.EndPhysics();
			physics.enabled = false;
		}

		// Update is called once per frame
		void OnPhysicsUpdate(long sampleTimestamp, Dictionary<uint, TransformProfile> snapshot) {
			//TODO: add a diffing method to reduce bandwidth
			bool absoluteNotDelta = /*lastSnapshot == null*/ true;
			
			//send the diff (no diff method for now)
			lastSnapshot = snapshot;

			StateUpdate update = new StateUpdate(sampleTimestamp, snapshot, absoluteNotDelta);
			NetworkServer.SendByChannelToAll(MsgTypeExt.STATE_UPDATE, new ByteMsgBase(update.ToBytes()), DefaultChannelExt.DEFAULT_FRAG_UNRELIABLE);
			Debug.Log("PHYSICS LATENCY: " + (float)(Utils.Timestamp - sampleTimestamp) / Utils.TICKS_PER_SEC);
		}

		/// <summary>
		/// Spawns a GameObject on all clients and registers its ownership
		/// /// </summary>
		/// <param name="ownerId">The owner connection id</param>
		/// <param name="obj">The object to spawn and register</param>
		public void Spawn(int ownerId, GameObject obj) {
			NetworkServer.Spawn(obj);
			Ownerships.RegisterOwnership(ownerId, obj);
			NetworkServer.connections[ownerId].SendByChannel(MsgTypeExt.OWNERSHIP, new UintMsgBase(obj.GetComponent<NetworkIdentity>().netId.Value), Channels.DefaultReliable);
		}
	}
}

