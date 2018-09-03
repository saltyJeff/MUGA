using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MUGA.Client {
	/// <summary>
	/// Performs basic interpolation between states
	/// </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	/// <seealso cref="MUGA.Client.IInterpolator" />
	class BasicInterpolator : MonoBehaviour, IInterpolator {
		private Queue<InterpolateStep> steps = new Queue<InterpolateStep>();
		private Queue<InterpolateStep> completeSteps = new Queue<InterpolateStep>();

		public float lerpDelay = 0.2f;

		private InterpolateStep from;
		private InterpolateStep to;
		public long interpTime;

		public float offServerTime;

		private void Start() {
			enabled = MUGAClient.isClient;
			Debug.Log(lerpDelay);
		}
		public void AcceptState(InterpolateStep step) {
			/*if(steps.Count > 0 && step.timestamp < steps.Peek().timestamp) {
				//throw away out-of-order messages
				return;
			}*/
			steps.Enqueue(step);
			completeSteps.Enqueue(step);
			offServerTime = (float)(Utils.Timestamp - step.timestamp) / Utils.TICKS_PER_SEC;
			Debug.Log("PARSE TIME: " + offServerTime);
		}
		private void Update() {
			if (steps.Count == 0) {
				return;
			}
			interpTime = (long)(Utils.Timestamp - (lerpDelay * Utils.TICKS_PER_SEC));
			from = GetFromStep();
			to = GetToStep();

			if (to != null && from != null) {
				double lerpVal = (interpTime - from.timestamp) / (to.timestamp - from.timestamp);
				LerpAllTheThings(from.profile, to.profile, (float)lerpVal);
			}
			else {
				//Debug.Log(to != null ? "Missing the future step" : "Missing the past step");
			}

		}
		private InterpolateStep GetFromStep() {
			if(steps.Count < 2) {
				return null;
			}
			InterpolateStep toReturn = null;
			InterpolateStep afterToReturn = steps.Peek();
			while(true) {
				if(afterToReturn.timestamp > interpTime) {
					break;
				}
				toReturn = steps.Dequeue();
				if(steps.Count > 0) {
					afterToReturn = steps.Peek();
				}
				else {
					break;
				}
			}
			return toReturn;
		}
		private InterpolateStep GetToStep() {
			if(steps.Count < 1) {
				return null;
			}
			return steps.Peek();
		}
		private void OnDrawGizmos() {
			Gizmos.color = Color.green;
			foreach (InterpolateStep step in completeSteps) {
				if(step == from) {
					Gizmos.color = Color.red;
					Gizmos.DrawWireCube(step.profile.position, Vector3.one);
				}
				else if(step == to) {
					Gizmos.color = Color.blue;
					Gizmos.DrawWireCube(step.profile.position, Vector3.one);
				}
				else {
					Gizmos.color = Color.white;
				}
			}
		}

		private void LerpAllTheThings(TransformProfile from, TransformProfile to, float lerpAmt) {
			transform.position = Vector3.Lerp(from.position, to.position, lerpAmt);
			transform.rotation = Quaternion.Lerp(from.rotation, to.rotation, lerpAmt);
			transform.localScale = Vector3.Lerp(from.localScale, to.localScale, lerpAmt);
		}
	}
}
