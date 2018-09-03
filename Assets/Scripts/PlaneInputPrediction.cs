using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MUGA.Client;
using MUGA;

public class PlaneInputPrediction : InputPredictionInterpolator {
	//stores position to spawn rockets
	public Transform rocketSpawnPos;
	public float lastRocketFire;
	public float lastLaserFire;

	public override InputSnapshot TakeInputSnapshot() {
		//here we create an InputSnapshot, copy any Input we want from the user at this frame, and return it
		//Utility methods are provided if you're just gonna copy from the Input class

		InputSnapshot snapshot = new InputSnapshot(Utils.Timestamp);
		CopyAxis("Horizontal", snapshot);
		CopyAxis("Vertical", snapshot);
		CopyButton("Fire1", snapshot);
		CopyButton("Fire2", snapshot);

		return snapshot;
	}
}
