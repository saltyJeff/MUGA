using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace MUGA.Server {
	//seperate into different class because I feel this will need changing
	/// <summary>
	/// Handles incoming inputs by parsing the byte[] and sending it to an input consumer
	/// </summary>
	class InputHandler {
		private InputConsumer inputConsumer;
		public InputHandler(InputConsumer inputConsumer) {
			this.inputConsumer = inputConsumer;
		}
		public void HandleInput(NetworkMessage msg) {
			InputSnapshot snapshot = MessagePackSerializer.Deserialize<InputSnapshot>(msg.ReadMessage<ByteMsgBase>().payload); //super sus
			inputConsumer.ConsumeInput(msg.conn.connectionId, snapshot);
		}
	}
}
