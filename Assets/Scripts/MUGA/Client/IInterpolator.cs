using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MUGA.Client {
	/// <summary>
	/// Interface for classes that need to recieve states to interpolate between
	/// </summary>
	public interface IInterpolator {
		void AcceptState(InterpolateStep step);
	}
	/// <summary>
	/// Represents relevant state information for a single class for a single state update
	/// </summary>
	public class InterpolateStep {
		public TransformProfile profile;
		public long timestamp;
		public InterpolateStep(TransformProfile prof, long ts) {
			profile = prof;
			timestamp = ts;
		}
	}
}
