using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class PlaneHP: NetworkBehaviour {
	[SyncVar]
	public int hp = 20;
	public TextMesh text;

	private void Update() {
		text.text = hp+"";
	}
}
