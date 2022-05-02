using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientCommon;

namespace Client01
{
	public class ServerPeer : Peer
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Dictionary<long, Handler> _handlers = new Dictionary<long, Handler>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void OnDisconnect()
		{
			Console.WriteLine("OnDisconnect.");
		}

		//
		//
		//

		protected override void ProcessOperationResponse(CommandName name, byte[] packet, long lnCommandId)
		{
			Handler handler = GetHandler(lnCommandId);
			if (handler == null)
			{
				Console.WriteLine("Not exist handler");
				return;
			}

			handler.Receive(packet);
			RemoveHandler(lnCommandId);
		}

		//
		//
		//

		private void AddHandler(Handler handler)
		{
			_handlers.Add(handler.Id, handler);
		}

		private Handler GetHandler(long lnId)
		{
			Handler value;

			return _handlers.TryGetValue(lnId, out value) ? value : null;
		}

		private void RemoveHandler(long lnId)
		{
			_handlers.Remove(lnId);
		}

		//
		//
		//

		public void Send(Handler handler)
		{
			AddHandler(handler);

			SendCommand(handler.CreateCommandParameters());
		}
	}
}
