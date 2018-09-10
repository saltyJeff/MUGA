using MUGA;
using MUGA.Client;
using MUGA.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DemoNetMan : NetworkManager {
	MUGAClient clientScript;
	MUGAServer serverScript;
	private void Start () {
		clientScript = MUGAClient.inst;
		serverScript = MUGAServer.inst;

		//set both to disabled when the game begins
		clientScript.enabled = false;
		serverScript.enabled = false;
	}
	public override void OnStartServer() {
		base.OnStartServer();
		//enable server script
		serverScript.enabled = true;
	}
	public override void OnServerReady(NetworkConnection conn) {
		base.OnServerReady(conn);
		//instantiate the player using the custom MUGA ownership system
		GameObject newPlayerObj = Instantiate(playerPrefab, GetStartPosition().position, Quaternion.identity);
		MUGAServer.inst.Spawn(conn.connectionId, newPlayerObj);
		//set the name of the object to the connId for EZ lookups
		newPlayerObj.name = conn.connectionId+"";
	}
	public override void OnServerDisconnect(NetworkConnection conn) {
		base.OnServerDisconnect(conn);
		//destroy the player
		List<GameObject> ownedByLeaving = Ownerships.GetOwnedObjs(conn.connectionId);
		foreach(GameObject obj in ownedByLeaving) {
			NetworkServer.Destroy(obj);
		}
		ownedByLeaving.Clear();
	}
	public override void OnStopServer() {
		//destroy everything
		foreach (List<GameObject> list in Ownerships.ownerships.Values) {
			foreach (GameObject obj in list) {
				NetworkServer.Destroy(obj);
			}
		}
		Ownerships.ownerships.Clear();
	}
	public override void OnStartClient(NetworkClient client) {
		base.OnStartClient(client);
		//enable client script
		clientScript.enabled = true;
		clientScript.BindToClient(client);

		//if we recieve a plane that is "ours", we want to enable input prediction on it and disable any other type of interpolation
		Ownerships.OnOwnership += (GameObject owned) => {
			owned.GetComponent<InputPredictionInterpolator>().enabled = true;
			BasicInterpolator otherInterpType = owned.GetComponent<BasicInterpolator>();
			if(otherInterpType != null) {
				otherInterpType.enabled = false;
			} 
		};
	}
}
