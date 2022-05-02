using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ClientSocket
{
	public class OperationRequest
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private byte m_bOperationCode = 0;
		private Dictionary<byte, object> m_parameters = new Dictionary<byte, object>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public OperationRequest() :
			this(0)
		{
		}

		public OperationRequest(byte bOperationCode) :
			this(bOperationCode, new Dictionary<byte, object>())
		{
		}

		public OperationRequest(byte bOperationCode, Dictionary<byte, object> parameters)
		{
			m_bOperationCode = bOperationCode;
			m_parameters = parameters;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public byte operationCode
		{
			get { return m_bOperationCode; }
			set { m_bOperationCode = value; }
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

		public static byte[] ToBytes(OperationRequest operationRequest)
		{
			MemoryStream stream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(operationRequest.operationCode);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, operationRequest.parameters);

			return stream.ToArray();
		}

		public static OperationRequest ToOperationRequest(byte[] bytes)
		{
			MemoryStream stream = new MemoryStream(bytes);
			BinaryReader reader = new BinaryReader(stream);
			byte bOperationCode = reader.ReadByte();

			BinaryFormatter formatter = new BinaryFormatter();
			object obj = formatter.Deserialize(stream);
			Dictionary<byte, object> parameters = (Dictionary<byte, object>)obj;

			return new OperationRequest(bOperationCode, parameters);
		}
	}
}
