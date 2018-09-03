using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using MUGA.Server;

namespace MUGA.Client {
	/// <summary>
	/// Interpolator that provides input prediction by saving Input Snapshots (disabled if on server)
	/// </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	/// <seealso cref="MUGA.Client.IInterpolator" />
	public abstract class InputPredictionInterpolator: MonoBehaviour, IInterpolator {
		//input handling
		public int maxClientSampleRate = 35;
		private long lastSend;
		private long ticksPerSend;
		private Queue<InputSnapshot> previousInputs = new Queue<InputSnapshot>();

		private InterpolateStep lastTruth;

		public float knownLag;
		public void AcceptState(InterpolateStep step) {
			if(lastTruth != null && step.timestamp < lastTruth.timestamp) {
				//throwaway out-of-order messages
				return;
			}
			long now = Utils.Timestamp;
			lastTruth = step;
			//new step is the truth
			lastTruth.profile.RestoreSelfToGameObject(gameObject);
			long lastKnownTime = lastTruth.timestamp;

			knownLag = (now - lastKnownTime) / Utils.TICKS_PER_SEC;
			if (previousInputs.Count < 1) {
				//Debug.Log("no saved inputs");
				return;
			}
			//deque all the ones older than this step
			InputSnapshot snap = previousInputs.Peek();
			while(snap != null && snap.timeSent < lastKnownTime) {
				previousInputs.Dequeue();
				if(previousInputs.Count < 1) {
					break;
				}
				snap = previousInputs.Peek();
			}

			//for the rest use physics magikery to simulate everything
			Physics.autoSimulation = false;

			//for each step simulate the futures
			foreach(InputSnapshot futureSnap in previousInputs) {
				//simulate teh future
				InputConsumer.inst.ConsumeInput(-1, futureSnap, false);
				Debug.Log(((float)futureSnap.timeSent - lastKnownTime) / Utils.TICKS_PER_SEC);
				Physics.Simulate((float)(futureSnap.timeSent - lastKnownTime) / Utils.TICKS_PER_SEC);
				lastKnownTime = futureSnap.timeSent;
			}

			Physics.autoSimulation = true;
		}
	
		public void Start () {
			enabled = MUGAClient.isClient;
			ticksPerSend = Utils.TICKS_PER_SEC / maxClientSampleRate;
		}

		public void Update () {
			long currentTime = Utils.Timestamp;
			if(currentTime > lastSend + ticksPerSend) { //perform rate limit
				lastSend = currentTime;
				InputSnapshot snapshot = TakeInputSnapshot();
				previousInputs.Enqueue(snapshot);
				InputConsumer.inst.ConsumeInput(-1, snapshot, true);
				MUGAClient.inst.client.SendByChannel(
					MsgTypeExt.USER_INPUT, 
					new ByteMsgBase(snapshot.ToBytes()), 
					Channels.DefaultUnreliable);
			}
		}
		private void OnDrawGizmos() {
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(transform.position, 2);
			if(lastTruth != null) {
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(lastTruth.profile.position, 2);
			}
		}

		/// <summary>
		/// Copies an axis's information from the Unity Input class to an InputSnapshot
		/// </summary>
		/// <param name="name">The name of the axis</param>
		/// <param name="input">The input snapshot to recieve the data.</param>
		public void CopyAxis(string name, InputSnapshot input) {
			input.SetAxis(name, Input.GetAxis(name));
		}
		/// <summary>
		/// Copies a button's information from the Unity Input class to an InputSnapshot
		/// </summary>
		/// <param name="name">The name of the button</param>
		/// <param name="input">The input snapshot to recieve the data.</param>
		public void CopyButton(string name, InputSnapshot input) {
			input.SetButton(name, Input.GetButton(name));
		}

		/// <summary>
		/// Takes the input snapshot.
		/// </summary>
		/// <returns>Returns a snapshot of the inputs on this frame</returns>
		public abstract InputSnapshot TakeInputSnapshot();
	}
}
