using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ClientSocket
{
	public class OperationResponse
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private byte m_bOperationCode = 0;
		private short m_nReturnCode = 0;
		private string m_sDebugMessage = "";
		private Dictionary<byte, object> m_parameters = new Dictionary<byte, object>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public OperationResponse() :
			this(0, 0)
		{
		}

		public OperationResponse(byte bOperationCode, short nReturnCode) :
			this(bOperationCode, nReturnCode, new Dictionary<byte, object>())
		{
		}

		public OperationResponse(byte bOperationCode, short nReturnCode, Dictionary<byte, object> parameters)
		{
			m_bOperationCode = bOperationCode;
			m_nReturnCode = nReturnCode;
			m_parameters = parameters;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public byte operationCode
		{
			get { return m_bOperationCode; }
			set { m_bOperationCode = value; }
		}

		public short returnCode
		{
			get { return m_nReturnCode; }
			set { m_nReturnCode = value; }
		}

		public string debugMessage
		{
			get { return m_sDebugMessage; }
			set { m_sDebugMessage = value; }
		}

		public Dictionary<byte, object> parameters
		{
			get { return m_parameters; }
			set { m_parameters = value; }
		}

		public object this[byte bIndex]
		{
			get { return m_parameters[bIndex]; }
			set { m_parameters[bIndex] = value; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static byte[] ToBytes(OperationResponse operationResponse)
		{
			MemoryStream stream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(operationResponse.operationCode);
			writer.Write(operationResponse.returnCode);
			writer.Write(operationResponse.debugMessage);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, operationResponse.parameters);

			return stream.ToArray();
		}

		public static OperationResponse ToOperationResponse(byte[] bytes)
		{
			MemoryStream stream = new MemoryStream(bytes);
			BinaryReader reader = new BinaryReader(stream);
			byte bOperationCode = reader.ReadByte();
			short nReturnCode = reader.ReadInt16();
			string sDebugMessage = reader.ReadString();

			BinaryFormatter formatter = new BinaryFormatter();
			object obj = formatter.Deserialize(stream);
			Dictionary<byte, object> parameters = (Dictionary<byte, object>)obj;

			OperationResponse response = new OperationResponse(bOperationCode, nReturnCode, parameters);
			response.debugMessage = sDebugMessage;

			return response;
		}
	}
}
