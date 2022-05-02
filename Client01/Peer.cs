using System;
using System.Collections.Generic;

using ClientSocket;
using ClientCommon;

namespace Client01
{
	public abstract class Peer : PeerBase
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnOperationResponse(OperationResponse operationResponse)
		{
			if (operationResponse.returnCode == 0)
			{
				CommandName name = (CommandName)operationResponse[(byte)CommandParameter.Name];
				byte[] packet = (byte[])operationResponse[(byte)CommandParameter.Packet];
				long lnCommandId = (long)operationResponse[(byte)CommandParameter.Id];

				ProcessOperationResponse(name, packet, lnCommandId);
			}
			else
			{
				Console.WriteLine("Error " + operationResponse.returnCode + " : " + operationResponse.debugMessage);
			}
		}

		protected override void OnEventData(EventData eventData)
		{
			//Console.WriteLine("OnEventData : " + (ServerEventName)eventData[(byte)ServerEventParameter.Name]);
		}

		//
		//
		//

		protected abstract void ProcessOperationResponse(CommandName name, byte[] packet, long lnCommandId);

		//
		//
		//

		public void SendCommand(Dictionary<byte, object> parameters)
		{
			SendOperationRequest(CreateRequest(RequestType.Command, parameters));
		}

		public void SendEvent(ClientEventName name, byte[] packet)
		{
			SendOperationRequest(CreateRequest(RequestType.Event, CreateEventParameters(name, packet)));
		}

		//
		//
		//

		private Dictionary<byte, object> CreateEventParameters(ClientEventName name, byte[] packet)
		{
			Dictionary<byte, object> parameters = new Dictionary<byte, object>();
			parameters[(byte)ClientEventParameter.Name] = (byte)name;
			parameters[(byte)ClientEventParameter.Packet] = packet;

			return parameters;
		}

		private OperationRequest CreateRequest(RequestType requestType, Dictionary<byte, object> parameters)
		{
			return new OperationRequest((byte)requestType, parameters);
		}
	}
}
