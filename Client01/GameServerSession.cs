using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using ClientCommon;

namespace Client01
{
	public class GameServerSession
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private ServerPeer m_serverPeer = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Constructors

		public GameServerSession(ServerPeer serverPeer)
		{
			if (serverPeer == null)
				throw new ArgumentNullException("serverPeer");

			m_serverPeer = serverPeer;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		//
		// 로그인
		//

		public void SendLoginCommand( string sAccessToken)
		{
			LoginCommandBody body = new LoginCommandBody();
			body.accessToken = sAccessToken;

			CommandHandler<LoginCommandBody, LoginResponseBody> handler = new CommandHandler<LoginCommandBody, LoginResponseBody>(CommandName.Login, body);
			handler.FinishHandler = OnLoginCommand;

			m_serverPeer.Send(handler);
		}

		private void OnLoginCommand(Handler handler)
		{
			SendLobbyInfo();
		}

		//
		// 로비정보
		//

		public void SendLobbyInfo()
		{
			CommandHandler<LobbyInfoCommandBody, LobbyInfoResponseBody> handler = new CommandHandler<LobbyInfoCommandBody, LobbyInfoResponseBody>(CommandName.LobbyInfo, null);
			handler.FinishHandler = OnLobbyInfoCommand;

			m_serverPeer.Send(handler);
		}

		private void OnLobbyInfoCommand(Handler handler)
		{
			LobbyInfoResponseBody resBody = (LobbyInfoResponseBody)handler.ResponseBody;
			PDLobbyHero[] heroes = resBody.heroes;

			if (heroes.Length == 0)
			{
				SendHeroCreate();
			}
			else
			{
				SendHeroLogin(heroes[0].heroId);
			}
		}

		//
		// 영웅생성
		//

		public void SendHeroCreate()
		{
			HeroCreateCommandBody body = new HeroCreateCommandBody();
			body.name = "클라이언트1";
			body.characterId = 1;

			CommandHandler<HeroCreateCommandBody, HeroCreateResponseBody> handler = new CommandHandler<HeroCreateCommandBody, HeroCreateResponseBody>(CommandName.HeroCreate, body);
			handler.FinishHandler = OnHeroCreateCommand;

			m_serverPeer.Send(handler);
		}

		private void OnHeroCreateCommand(Handler handler)
		{
			SendLobbyInfo();
		}

		//
		// 영웅로그인
		//

		public void SendHeroLogin(Guid heroId)
		{
			HeroLoginCommandBody body = new HeroLoginCommandBody();
			body.heroId = heroId;

			CommandHandler<HeroLoginCommandBody, HeroLoginResponseBody> handler = new CommandHandler<HeroLoginCommandBody, HeroLoginResponseBody>(CommandName.HeroLogin, body);
			handler.FinishHandler = OnHeroLogin;

			m_serverPeer.Send(handler);
		}

		private void OnHeroLogin(Handler handler)
		{
			SendHeroInitEnter();
		}

		//
		// 영웅초기입장
		//

		private Guid placeInstanceId;
		public void SendHeroInitEnter()
		{
			CommandHandler<HeroInitEnterCommandBody, HeroInitEnterResponseBody> handler = new CommandHandler<HeroInitEnterCommandBody, HeroInitEnterResponseBody>(CommandName.HeroInitEnter, null);
			handler.FinishHandler = OnHeroInitEnter;

			m_serverPeer.Send(handler);
		}

		private void OnHeroInitEnter(Handler handler)
		{
			Console.WriteLine("OnHeroInitEnter");

			HeroInitEnterResponseBody resBody = (HeroInitEnterResponseBody)handler.ResponseBody;
			placeInstanceId = resBody.placeInstanceId;

			SendHeroMoveStart();
		}

		//
		// 영웅이동시작
		//

		public void SendHeroMoveStart()
		{
			HeroMoveStartCommandBody body = new HeroMoveStartCommandBody();
			body.placeInstanceId = placeInstanceId;

			CommandHandler<HeroMoveStartCommandBody, HeroMoveStartResponseBody> handler = new CommandHandler<HeroMoveStartCommandBody, HeroMoveStartResponseBody>(CommandName.HeroMoveStart, body);
			handler.FinishHandler = OnHeroMoveStart;

			m_serverPeer.Send(handler);
		}

		private void OnHeroMoveStart(Handler handler)
		{
			ThreadPool.QueueUserWorkItem(Move);
		}

		private void Move(object state)
		{
			while (true)
			{
				SendHeroMove();
				Thread.Sleep(300);
			}
		}

		//
		// 영웅이동
		//

		private int nIndex = -1;
		private bool bIsBack = false;
		public void SendHeroMove()
		{
			if (bIsBack)
			{
				if (nIndex == 0)
					bIsBack = false;
			}
			else
			{
				if (nIndex >= Program.movePosition.Count - 1)
					bIsBack = true;
			}

			if (bIsBack)
				nIndex--;
			else
				nIndex++;

			Vector3 position = Program.movePosition[nIndex];

			HeroMoveCommandBody body = new HeroMoveCommandBody();
			body.placeInstanceId = placeInstanceId;
			body.position = new PDVector3(position.x, position.y, position.z);
			body.yRotation = 0;

			CommandHandler<HeroMoveCommandBody, HeroMoveResponseBody> handler = new CommandHandler<HeroMoveCommandBody, HeroMoveResponseBody>(CommandName.HeroMove, body);
			handler.FinishHandler = OnHeroMove;

			m_serverPeer.Send(handler);
		}

		private void OnHeroMove(Handler handler)
		{
		}

		//
		// 영웅이동종료
		//

		public void SendHeroMoveEnd(Guid placeInstanceId, Vector3 position, float fYRotation)
		{
			HeroMoveEndCommandBody body = new HeroMoveEndCommandBody();
			body.placeInstanceId = placeInstanceId;
			body.position = new PDVector3(position.x, position.y, position.z);
			body.yRotation = fYRotation;

			CommandHandler<HeroMoveEndCommandBody, HeroMoveEndResponseBody> handler = new CommandHandler<HeroMoveEndCommandBody, HeroMoveEndResponseBody>(CommandName.HeroMoveEnd, body);
			handler.FinishHandler = OnHeroEndMove;

			m_serverPeer.Send(handler);
		}

		private void OnHeroEndMove(Handler handler)
		{
			//Debug.Log("OnHeroMoveEnd");
		}
	}
}
