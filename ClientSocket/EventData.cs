using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ClientSocket
{
	public class EventData
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private byte m_bCode = 0;
		private Dictionary<byte, object> m_parameters = new Dictionary<byte, object>();

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public EventData()
			: this(0)
		{
		}

		public EventData(byte bCode)
			: this(bCode, new Dictionary<byte, object>())
		{
		}

		public EventData(byte bCode, Dictionary<byte, object> parameters)
		{
			m_bCode = bCode;
			m_parameters = parameters;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public byte code
		{
			get { return m_bCode; }
		}

		public Dictionary<byte, object> parameters
		{
			get { return m_parameters; }
		}

		public object this[byte bIndex]
		{
			get { return m_parameters[bIndex]; }
			set { m_parameters[bIndex] = value; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static byte[] ToBytes(EventData eventData)
		{
			MemoryStream stream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(eventData.code);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, eventData.parameters);

			return stream.ToArray();
		}

		public static EventData ToEventData(byte[] bytes)
		{
			MemoryStream stream = new MemoryStream(bytes);
			BinaryReader reader = new BinaryReader(stream);
			byte bCode = reader.ReadByte();

			BinaryFormatter formatter = new BinaryFormatter();
			object obj = formatter.Deserialize(stream);
			Dictionary<byte, object> parameters = (Dictionary<byte, object>)obj;

			return new EventData(bCode, parameters);
		}
	}
}
