using System;
using System.IO;

namespace ClientSocket
{
	public class Data
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		public const byte STX = 0x02;
		public const byte ETX = 0x03;

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

			byte[] buffer = WriteBuffer(new byte[][] { new byte[] { STX }, new byte[] { (byte)data.type }, BitConverter.GetBytes(data.packetLength), data.packet, new byte[2], new byte[] { ETX } });

			//
			// 체크섬(순환중복검사)
			//

			byte[] checkArray = new byte[buffer.Length - 4];
			Array.Copy(buffer, 1, checkArray, 0, checkArray.Length);
			byte[] crc = BitConverter.GetBytes(Crc16.Calc(checkArray));
			buffer[buffer.Length - 3] = crc[0];
			buffer[buffer.Length - 2] = crc[1];

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

			//
			// 체크섬 검사
			//

			if (buffer[0] != STX || buffer[nLength - 1] != ETX)
				return false;

			byte[] checkArray = new byte[nLength - 4];
			Array.Copy(buffer, 1, checkArray, 0, checkArray.Length);
			byte[] crc = BitConverter.GetBytes(Crc16.Calc(checkArray));

			if (buffer[nLength - 3] != crc[0] || buffer[nLength - 2] != crc[1])
				return false;


			//
			// 패킷 타입
			//

			byte bType = buffer[1];

			//
			// 패킷 길이
			//

			int nPacketLength = (int)(buffer[2] | buffer[3] << 8 | buffer[4] << 16 | buffer[5] << 24);
			byte[] packet = new byte[nPacketLength];
			Array.Copy(buffer, 6, packet, 0, nPacketLength);

			data.Set((PacketType)bType, packet);

			return true;
		}
	}
}
