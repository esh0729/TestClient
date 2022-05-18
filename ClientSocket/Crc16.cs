using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSocket
{
	public class Crc16
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public static ushort Calc(byte[] packet)
		{
			ushort usCRC = 0xFFFF;
			ushort usTemp = 0;

			foreach (byte bPacket in packet)
			{
				// 하위 4비트에 대한 체크섬 계산
				usTemp = s_Crc16Table[usCRC & 0x000F];
				usCRC = (ushort)((usCRC >> 4) & 0x0FFF);
				usCRC = (ushort)(usCRC ^ usTemp ^ s_Crc16Table[bPacket & 0x000F]);
				// 상위 4비트에 대한 체크섬 계산
				usTemp = s_Crc16Table[usCRC & 0x000F];
				usCRC = (ushort)((usCRC >> 4) & 0x0FFF);
				usCRC = (ushort)(usCRC ^ usTemp ^ s_Crc16Table[(bPacket >> 4) & 0x000F]);
			}

			return usCRC;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Static member variables

		private static ushort[] s_Crc16Table = { 0x0000, 0xCC01, 0xD801, 0x1400, 0xF001, 0x3C00, 0x2800, 0xE401, 0xA001, 0x6C00, 0x7800, 0xB401, 0x5000, 0x9C01, 0x8801, 0x4400 };
	}
}
