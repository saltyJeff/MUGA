using MUGA;
using MUGA.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DemoInputConsumer : InputConsumer {
	public float speed = 4;
	public GameObject rocketPrefab;
	public float rocketFireRate = 0.66f;
	public float rocketSpeed = 18;

	public float laserFireRate = 0.33f;

	//Process incoming input snapshots
	public override void ConsumeInput(int connectionId, InputSnapshot input, bool firstExec) {
		//objsOwned will either be empty (we return) or contain our plane
		List<GameObject> objsOwned = Ownerships.GetOwnedObjs(connectionId);
		if(objsOwned.Count == 0) {
			return;
		}
		else if(objsOwned.Count > 1) {
			//in case I messed up the MUGA code
			Debug.LogError(new System.InvalidProgramException("How do we own more planes?"));
		}
		//get the rigidbody and change its velocity
		Rigidbody myPlane = objsOwned[0].GetComponent<Rigidbody>();
		myPlane.velocity = new Vector3(input.GetAxis("Horizontal") * speed, 0, input.GetAxis("Vertical") * speed);
		//we want to apply rotation only if the velocity is signifigant
		if(myPlane.velocity.sqrMagnitude > 1) {
			myPlane.transform.forward = myPlane.velocity.normalized;
		}
		
		//for firing rockets and guns, we want to execute the creation code only on the server
		if(connectionId == -1) {
			return;
		}
		PlaneInputPrediction inputPrediction = myPlane.GetComponent<PlaneInputPrediction>();
		/*
		 * it is safe to use Time.time here because this information wil only be used on the server
		 * If you want to use time that will be sync'd between client and server, use
		 * Utils.Timestamp in the MUGA package which will fetch the OS time
		 */
		if (input.GetButton("Fire1") && Time.time > inputPrediction.lastRocketFire + rocketFireRate) {
			//get rocket spawn position and spawn it
			Vector3 rocketLocation = inputPrediction.rocketSpawnPos.position;
			GameObject newRocket = Instantiate(rocketPrefab, rocketLocation, Quaternion.identity);
			newRocket.transform.forward = myPlane.transform.forward;

			newRocket.GetComponent<Rigidbody>().velocity = myPlane.transform.forward * rocketSpeed;

			//Notify clients of the rocket
			NetworkServer.Spawn(newRocket);

			//Reset the fire rate limiter
			inputPrediction.lastRocketFire = Time.time;
		}
		if(input.GetButton("Fire2") && Time.time > inputPrediction.lastLaserFire + laserFireRate) {
			//get rocket spawn position and spawn it
			Vector3 rocketLocation = inputPrediction.rocketSpawnPos.position;
			//get an instance of "Restorer" to represent the past state
			using(Restorer rest = LCPhysics.GetRestorer(input.timeSent)) {
				RaycastHit hit;
				if(Physics.Raycast(rocketLocation, myPlane.transform.forward, out hit)) {
					if(hit.collider.tag == "Player") {
						GameObject enemy = Ownerships.GetOwnedObjs(int.Parse(hit.collider.name))[0];
						enemy.GetComponent<PlaneHP>().hp -= 4;
					}
					Debug.Log("LC hit " + hit.collider.tag);
				}
			}
			
			//Reset the fire rate limiter
			inputPrediction.lastLaserFire = Time.time;
		}
	}
}
