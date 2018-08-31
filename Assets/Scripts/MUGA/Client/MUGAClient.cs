using MessagePack;
using MUGA.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MUGA.Client {
	/// <summary>
	/// Client side script (disable if on Server)
	/// </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public class MUGAClient: MonoBehaviour {
		//public use for the scripts
		public static bool isClient {
			get {
				return inst.enabled;
			}
			set {
				//thowaway
			}
		}
		public static MUGAClient inst;

		//interpolation handling
		public int interpBufferSize = 3;
		private Dictionary<uint, IInterpolator> interpolatorHandlers = new Dictionary<uint, IInterpolator>();
		public static float interpTime = -1; //negative = not intialized yet
		public float offServerTime;

		public NetworkClient client;

		private void Awake() {
			if(inst == null) {
				inst = this;
			}
			else {
				throw new InvalidOperationException("There must be more than 1 MUGAClient class in the scene. There can only be 1");
			}
			Ownerships.OnOwnership += (GameObject go) => {
				InputPredictionInterpolator interp = go.GetComponent<InputPredictionInterpolator>();
				if (interp != null) {
					interp.enabled = true;
				}
			};
		}

		public void HandleStateUpdate(NetworkMessage msg) {
			StateUpdate snapshot = MessagePackSerializer.Deserialize<StateUpdate>(msg.ReadMessage<ByteMsgBase>().payload);
			long offServerTicks = Utils.Timestamp - snapshot.timestamp;
			offServerTime = (float)offServerTicks / Utils.TICKS_PER_SEC;

			//TODO: latency interpolation
			foreach(KeyValuePair<uint, TransformProfile> pair in snapshot.gameObjectProfiles) {
				IInterpolator interpolator;
				if(!interpolatorHandlers.TryGetValue(pair.Key, out interpolator)) {
					GameObject obj = ClientScene.FindLocalObject(new NetworkInstanceId(pair.Key));
					if (obj == null) {
						Debug.LogWarning("Object " + pair.Value.name + " (id: " + pair.Key + ") was not found on the client, skipping it");
						continue;
					}
					IInterpolator component = obj.GetComponent<IInterpolator>();
					if(component == null) {
						component = obj.AddComponent<BasicInterpolator>();
					}
					interpolator = component;
					interpolatorHandlers[pair.Key] = component;
				}
				interpolator.AcceptState(new InterpolateStep(pair.Value, snapshot.timestamp));
			}

			if(interpTime < 0) {
				interpTime = LCPhysics.GetTicksPerSample() * interpBufferSize;
				Debug.Log("interpolation time:  "+interpTime / Utils.TICKS_PER_SEC);
			}
		}

		public void BindToClient(NetworkClient client) {
			client.RegisterHandler(MsgTypeExt.STATE_UPDATE, HandleStateUpdate);
			//perform custom ownership system (client)
			client.RegisterHandler(MsgTypeExt.OWNERSHIP, (msg) => {
				//-1 means local client "ownership"
				Ownerships.RegisterOwnership(-1, ClientScene.FindLocalObject(new NetworkInstanceId(msg.ReadMessage<UintMsgBase>().val)));
			});
			this.client = client;
		}
	}
}

