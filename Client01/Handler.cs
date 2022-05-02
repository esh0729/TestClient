using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientCommon;

namespace Client01
{
	public abstract class Handler
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		protected long _lnId = 0;
		protected Action<Handler> _finishHandler = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public Handler()
		{
			_lnId = CreateId();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public long Id
		{
			get { return _lnId; }
		}

		public Action<Handler> FinishHandler
		{
			set { _finishHandler = value; }
		}

		public abstract ResponseBody ResponseBody { get; }

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Receive(byte[] packet)
		{
			CreateResponseBody(packet);

			if (_finishHandler != null)
				_finishHandler(this);
		}

		protected abstract void CreateResponseBody(byte[] packet);

		public abstract Dictionary<byte, object> CreateCommandParameters();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		private static long _lnCreationId = 0;
		private object _idCreationObject = new object();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		private long CreateId()
		{
			lock (_idCreationObject)
			{
				return _lnCreationId++;
			}
		}
	}
}
