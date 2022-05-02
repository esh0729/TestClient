using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientCommon;

namespace Client01
{
	public class CommandHandler<T1, T2> : Handler
	where T1 : CommandBody where T2 : ResponseBody
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private CommandName _commandName;

		private T1 _commandBody = null;
		private T2 _responseBody = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public CommandHandler(CommandName commandName, T1 commandBody)
			: base()
		{
			_commandName = commandName;
			_commandBody = commandBody;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public override ResponseBody ResponseBody
		{
			get { return _responseBody; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		protected override void CreateResponseBody(byte[] packet)
		{
			_responseBody = Activator.CreateInstance<T2>();
			_responseBody.DeserializeRaw(packet);
		}

		public override Dictionary<byte, object> CreateCommandParameters()
		{
			Dictionary<byte, object> parameters = new Dictionary<byte, object>();
			parameters[(byte)CommandParameter.Name] = (byte)_commandName;
			parameters[(byte)CommandParameter.Packet] = _commandBody != null ? _commandBody.SerializeRaw() : new byte[] { };
			parameters[(byte)CommandParameter.Id] = _lnId;

			return parameters;
		}
	}
}
