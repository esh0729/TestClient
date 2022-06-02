using System;
using System.IO;

namespace ClientSocket
{
	public class Data
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		public const int kLengthSize = 4;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		public PacketType m_type;
		public int m_nPacketLength;
		public byte[] m_packet;

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
		// Member functions

		public void Set(PacketType type, byte[] packet)
		{
			if (packet == null)
				throw new ArgumentNullException("packet");

			m_type = type;
			m_nPacketLength = packet.Length;
			m_packet = packet;
		}

		public void Clear()
		{
			m_type = default(PacketType);
			m_nPacketLength = 0;
			m_packet = null;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member functions

		public static byte[] ToBytes(Data data)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			//
			// 버퍼에 데이터 삽입
			//

			byte[] buffer = WriteBuffer(new byte[][] { new byte[4], new byte[] { (byte)data.type }, BitConverter.GetBytes(data.packetLength), data.packet, new byte[2] });

			//
			// 체크섬(순환중복검사)
			//

			int nBufferLength = buffer.Length;
			ushort usChecksum = Crc16.Calc(buffer, kLengthSize, nBufferLength - 2);
			buffer[nBufferLength - 2] = (byte)(0x00ff & usChecksum);
			buffer[nBufferLength - 1] = (byte)(0x00ff & (usChecksum >> 8));

			//
			// 전체 길이
			//

			buffer[0] = (byte)(0x000000ff & nBufferLength);
			buffer[1] = (byte)(0x000000ff & (nBufferLength >> 8));
			buffer[2] = (byte)(0x000000ff & (nBufferLength >> 16));
			buffer[3] = (byte)(0x000000ff & (nBufferLength >> 24));

			return buffer;
		}

		private static byte[] WriteBuffer(byte[][] datas)
		{
			//
			// 모든 데이터의 길이만큼 패킷배열 생성
			//

			int nLength = 0;
			foreach (byte[] data in datas)
			{
				nLength += data.Length;
			}

			//
			// 패킷 배열에 데이터 삽입
			//

			byte[] packet = new byte[nLength];
			int nIndex = 0;
			foreach (byte[] data in datas)
			{
				Array.Copy(data, 0, packet, nIndex, data.Length);
				nIndex += data.Length;
			}

			return packet;
		}

		public static bool ToData(byte[] buffer, int nLength, ref Data data)
		{
			if (buffer == null)
				return false;

			int nCurrentIndex = kLengthSize;

			//
			// 체크섬(순환중복검사)
			//

			ushort usChecksum = Crc16.Calc(buffer, nCurrentIndex, nLength - 2);
			if (buffer[nLength - 2] != (byte)(0x00ff & usChecksum) || buffer[nLength - 1] != (byte)(0x00ff & (usChecksum >> 8)))
				return false;

			//
			// 패킷 타입
			//

			byte bType = buffer[nCurrentIndex++];

			//
			// 패킷 길이
			//

			int nPacketLength = (int)(buffer[nCurrentIndex++] | buffer[nCurrentIndex++] << 8 | buffer[nCurrentIndex++] << 16 | buffer[nCurrentIndex++] << 24);
			byte[] packet = new byte[nPacketLength];
			Array.Copy(buffer, nCurrentIndex, packet, 0, nPacketLength);

			data.Set((PacketType)bType, packet);

			return true;
		}
	}
}
