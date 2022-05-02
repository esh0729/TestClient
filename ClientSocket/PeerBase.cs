using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ClientSocket
{
	public abstract class PeerBase
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constants

		private const int kPingCheckInterval = 500;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Socket m_socket = null;

		private object m_sendSyncObject = new object();
		private Queue<OperationRequest> m_operationRequests = new Queue<OperationRequest>();

		private DateTime m_lastSendPingCheckTime = DateTime.MinValue;
		private DateTime m_lastReceivePingCheckTime = DateTime.MinValue;

		private int m_nConnectionTimeoutInterval = 0;

		private bool m_bIsDisconnected;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public void Start(string sAddress, int nPort, int nConnectionTimeoutInterval = 30000)
		{
			m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(sAddress), nPort);
			m_socket.Connect(remoteEP);

			m_nConnectionTimeoutInterval = nConnectionTimeoutInterval;
			m_lastReceivePingCheckTime = DateTime.Now;
		}

		public bool Service()
		{
			if (m_bIsDisconnected)
				return false;

			//
			// Timeout 시간 갱신
			//

			SendPing();
			
			//
			// Reiceve
			//

			Receive();
			
			//
			// Queue에 저장된 Request 서버로 전달
			//

			Send();

			//
			// Timeout 시간 체크
			//

			if (m_nConnectionTimeoutInterval != 0 && (DateTime.Now - m_lastReceivePingCheckTime).TotalMilliseconds > (double)m_nConnectionTimeoutInterval)
			{
				Disconnect("Timeout.");
				return false;
			}

			return true;
		}

		private void Receive()
		{
			try
			{
				int available = m_socket.Available;
				if (available <= 0)
					return;

				//
				// 읽기 가능한 데이터가 1바이트 경우 종료처리
				//

				if (available == 1)
				{
					Disconnect("Server Exit.");
					return;
				}

				//
				// 4바이트 Packet의 총 바이트
				//

				byte[] buffer = new byte[sizeof(int)];
				if (m_socket.Receive(buffer, buffer.Length, SocketFlags.None) > 0)
				{
					int nBufferLength = BitConverter.ToInt32(buffer, 0);

					//
					// 방금 받은 바이트수 만큼만 읽기 처리
					//

					buffer = new byte[nBufferLength];

					m_socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

					//
					// 역직렬화
					//

					FullPacket fullPacket = FullPacket.ToFullPacket(buffer);

					switch (fullPacket.type)
					{
						// Timeout 갱신 처리
						case PacketType.PingCheck: OnReceivePingCheck(); break;

						// 서버 이벤트 처리
						case PacketType.EventData: OnEventData(EventData.ToEventData(fullPacket.packet)); break;

						// 커맨드 응답 처리
						case PacketType.OperationResponse: OnOperationResponse(OperationResponse.ToOperationResponse(fullPacket.packet)); break;

						default:
							throw new Exception("Not Valied PacketType");
					}
				}
				else
				{
					//
					// 서버에서 소켓 Close하였을 경우 0바이트 전송(동기처리 위해 읽기 가능한 바이트수 체크하기 때문에 처리X, Timeout 또는 1바이트로 종료 체크)
					//
				
					Disconnect("ServerSocket Disconnect");
				}
			}
			catch (Exception ex)
			{
				//
				// 소켓 Reiceve중 에러 처리 이후 접속이 끊어졌을 경우 Disconnect처리
				//

				if (!m_socket.Connected)
					Disconnect("ReceiveError.");

				OnReceviceError(ex);
			}
		}

		protected virtual void OnReceviceError(Exception ex)
		{
		}

		protected abstract void OnEventData(EventData eventData);

		protected abstract void OnOperationResponse(OperationResponse operationResponse);

		private void SendPing()
		{
			//
			// 서버에 Timeout시간 체크용 빈데이터 전달(서브는 Request Receive 처리 시간으로 클라이언트는 Response Receive 처리 시간으로 Timeout시간 갱신)
			//

			try
			{
				DateTime now = DateTime.Now;
				if ((now - m_lastSendPingCheckTime).TotalMilliseconds > kPingCheckInterval)
				{
					m_lastSendPingCheckTime = now;

					List<byte> fullBuffer = new List<byte>();

					FullPacket fullPacket = new FullPacket(PacketType.PingCheck, new byte[] { });
					byte[] buffer = FullPacket.ToBytes(fullPacket);

					fullBuffer.AddRange(BitConverter.GetBytes(buffer.Length));
					fullBuffer.AddRange(buffer);

					m_socket.Send(fullBuffer.ToArray());
				}
			}
			catch (Exception ex)
			{
				OnSendPingError(ex);
			}
		}

		//
		// PingCheck Response Timeout 시간 갱신
		//

		private void OnReceivePingCheck()
		{
			m_lastReceivePingCheckTime = DateTime.Now;
		}

		protected virtual void OnSendPingError(Exception ex)
		{
		}

		private void Send()
		{
			try
			{
				lock (m_sendSyncObject)
				{
					//
					// RequestQueue에 데이터가 있을경우 데이터 전송
					//

					if (m_operationRequests.Count != 0)
					{
						List<byte> fullBuffer = new List<byte>();

						while (m_operationRequests.Count > 0)
						{
							OperationRequest operationRequest = m_operationRequests.Peek();

							//
							// Request 데이터 직렬화
							//

							byte[] buffer = FullPacket.ToBytes(new FullPacket(PacketType.OperationRequest, OperationRequest.ToBytes(operationRequest)));

							//
							// Packet의 바이트수 + Packet 서버에 전달
							//

							fullBuffer.AddRange(BitConverter.GetBytes(buffer.Length));
							fullBuffer.AddRange(buffer);

							m_operationRequests.Dequeue();
						}

						m_socket.Send(fullBuffer.ToArray());
					}
				}
			}
			catch (Exception ex)
			{
				OnSendError(ex);
			}
		}

		protected virtual void OnSendError(Exception ex)
		{
		}

		protected void SendOperationRequest(OperationRequest operationRequest)
		{
			lock (m_sendSyncObject)
			{
				m_operationRequests.Enqueue(operationRequest);
			}
		}

		public void Disconnect(string sType)
		{
			Console.WriteLine("DisconnectType = " + sType);

			if (!m_bIsDisconnected)
			{
				m_bIsDisconnected = true;

				try
				{
					m_socket.Disconnect(reuseSocket: true);
					m_socket.Close();
				}
				finally
				{
					OnDisconnect();
				}
			}
		}

		protected abstract void OnDisconnect();
	}
}
