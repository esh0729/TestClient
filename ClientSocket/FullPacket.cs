using System;
using System.IO;

namespace ClientSocket
{
	public class FullPacket
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public PacketType m_type;
		public int m_nPacketLength;
		public byte[] m_packet;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public FullPacket(PacketType type, byte[] packet)
		{
			if (packet == null)
				throw new ArgumentNullException("packet");

			m_type = type;
			m_nPacketLength = packet.Length;
			m_packet = packet;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public PacketType type
		{
			get { return m_type; }
		}

		public int packetLength
		{
			get { return m_nPacketLength; }
		}

		public byte[] packet
		{
			get { return m_packet; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static byte[] ToBytes(FullPacket fullPacket)
		{
			MemoryStream stream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write((byte)fullPacket.type);
			writer.Write(fullPacket.packetLength);
			writer.Write(fullPacket.packet);

			return stream.ToArray();
		}

		public static FullPacket ToFullPacket(byte[] bytes)
		{
			MemoryStream stream = new MemoryStream(bytes);
			BinaryReader reader = new BinaryReader(stream);
			byte bType = reader.ReadByte();
			int nPacketLength = reader.ReadInt32();
			byte[] packet = reader.ReadBytes(nPacketLength);

			return new FullPacket((PacketType)bType, packet);
		}
	}
}
