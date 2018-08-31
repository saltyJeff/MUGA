using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;

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

		public float lerpAmt = 0.1f;
		private TransformProfile goal;

		private InterpolateStep lastTruth;
		public float timeSim;

		public void AcceptState(InterpolateStep step) {
			if(lastTruth != null && step.timestamp < lastTruth.timestamp) {
				//throwaway out-of-order messages
				return;
			}
			long now = Utils.Timestamp;
			lastTruth = step;
			//hold the old state
			TransformProfile start = new TransformProfile(this.gameObject);
			//new step is the truth
			lastTruth.profile.RestoreSelfToGameObject(gameObject);

			if(previousInputs.Count < 1) {
				Debug.Log("no saved inputs");
				return;
			}
			//deque all the ones older than this step
			InputSnapshot snap = previousInputs.Peek();
			while(snap.timeSent < step.timestamp) {
				previousInputs.Dequeue();
				if(previousInputs.Count < 1) {
					break;
				}
				snap = previousInputs.Peek();
			}

			//for the rest use physics magikery to simulate everything
			Physics.autoSimulation = false;

			timeSim = 0;
			//for each step simulate the futures
			foreach(InputSnapshot futureSnap in previousInputs) {
				//simulate teh future
				InputConsumer.inst.ConsumeInput(-1, futureSnap);
				Physics.Simulate((float)ticksPerSend / Utils.TICKS_PER_SEC);
				timeSim += (float)ticksPerSend / Utils.TICKS_PER_SEC;
			}

			//save the new state as goal
			goal = new TransformProfile(this.gameObject);

			//restore the old state
			start.RestoreSelfToGameObject(this.gameObject);

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
				MUGAClient.inst.client.SendByChannel(
					MsgTypeExt.USER_INPUT, 
					new ByteMsgBase(snapshot.ToBytes()), 
					Channels.DefaultUnreliable);
			}
			//attempt to "smooth" any prediction error
			if(goal != null) {
				transform.position = Vector3.Lerp(goal.position, transform.position, lerpAmt);
				transform.rotation = Quaternion.Lerp(goal.rotation, transform.rotation, lerpAmt);
				transform.localScale = Vector3.Lerp(goal.localScale, transform.localScale, lerpAmt);
			}
		}
		private void OnDrawGizmos() {
			if(goal != null) {
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(goal.position, 1);
			}
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
