using MUGA.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RocketScript : MonoBehaviour {
	//Using Time.time here since this script works only on the server
	public float creationTime;
	public float lifetime = 3;
	// Use this for initialization
	void Start () {
		if(MUGAClient.isClient) {
			enabled = false;
			return;
		}
		creationTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if(Time.time > creationTime + lifetime) {
			NetworkServer.Destroy(gameObject);
		}
	}

	private void OnCollisionEnter(Collision collision) {
		//destroy self if hit wall
		if(collision.collider.tag == "Wall") {
			NetworkServer.Destroy(gameObject);
		}
		//destroy self and other rocket if hits another rocket
		else if(collision.collider.tag == "Rocket") {
			NetworkServer.Destroy(collision.gameObject);
			NetworkServer.Destroy(gameObject);
		}
		else if(collision.collider.tag == "Player") {
			collision.collider.GetComponentInParent<PlaneHP>().hp -= 5;
		}
	}
}
