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
		private InterpolateStep currentStep;
		public float lerpAmt;
		private void Start() {
			enabled = MUGAClient.isClient;
		}
		public void AcceptState(InterpolateStep step) {
			if(steps.Count > 0 && step.timestamp < steps.Peek().timestamp) {
				//throwaway out-of-order messages
				return;
			}
			steps.Enqueue(step);
		}
		private void Update() {
			if (steps.Count == 0) {
				return;
			}
			if (currentStep == null) {
				currentStep = steps.Dequeue();
				currentStep.profile.RestoreSelfToGameObject(this.gameObject);
			}
			else {
				InterpolateStep nextStep = steps.Peek();
				lerpAmt = (nextStep.timestamp - (Utils.Timestamp - MUGAClient.interpTime)) / MUGAClient.interpTime;
				//0 lerpAmt = nextStep, 1 = currentStep) it's kinda flipped
				LerpAllTheThings(nextStep.profile, lerpAmt);
				if(lerpAmt < 0) {
					//other non-lerpable things that may have changed
					tag = nextStep.profile.tag;
					gameObject.layer = nextStep.profile.layer;

					currentStep = steps.Dequeue();

					if(steps.Count == 0) {
						return;
					}
					//if the lerp is negative then we should proabably skip over to the next one
					nextStep = steps.Peek();
					
					LerpAllTheThings(nextStep.profile, 1.0f+lerpAmt);
				}
			}
		}
		private void OnDrawGizmos() {
			foreach (InterpolateStep step in steps) {
				Gizmos.DrawWireCube(step.profile.position, Vector3.one);
			}
		}
		/// <summary>
		/// Lerps transform position, rotation, and scale
		/// </summary>
		/// <param name="prof">The profile to lerp from</param>
		/// <param name="lerpAmt">The lerp amount (0 = profile position and 1 = currentStep)</param>
		public void LerpAllTheThings(TransformProfile prof, float lerpAmt) {
			transform.position = Vector3.Lerp(prof.position, currentStep.profile.position, lerpAmt);
			transform.rotation = Quaternion.Lerp(prof.rotation, currentStep.profile.rotation, lerpAmt);
			transform.localScale = Vector3.Lerp(prof.localScale, currentStep.profile.localScale, lerpAmt);
		}
	}
}
