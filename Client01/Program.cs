using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Net;
using System.IO;
using LitJson;

namespace Client01
{
	public class Program
	{
        public static List<Vector3> movePosition = new List<Vector3>();

        private static void Main()
		{
            ReadMovePosition();

            int nMinWorkerThreads;
            int nMaxWorkerThreads;
            int nMinCompletionPortThreads;
            int nMaxCompletionPortThreads;

            ThreadPool.GetMinThreads(out nMinWorkerThreads, out nMinCompletionPortThreads);
            ThreadPool.GetMaxThreads(out nMaxWorkerThreads, out nMaxCompletionPortThreads);

            //Console.WriteLine("nMinWorkerThreads = " + nMinWorkerThreads + ", nMaxWorkerThreads = " + nMaxWorkerThreads + ", nMinCompletionPortThreads = " + nMinCompletionPortThreads + ", nMaxCompletionPortThreads = " + nMaxCompletionPortThreads);
            ThreadPool.SetMinThreads(nMaxWorkerThreads, nMinCompletionPortThreads);

            Console.Write("클라이언트 수 : ");
            int nCount = Convert.ToInt32(Console.ReadLine());

            for (int n = 0; n < nCount; n++)
			{
                Thread.Sleep(1000);
                ThreadPool.QueueUserWorkItem(Update);
			}

            Console.ReadLine();
        }

        private static void Update(object state)
		{
            //
            // 시스템정보
            //

            if (!LoadSystemInfo())
                return;

            //
            // 메타데이터
            //

            if (!LoadMetaData())
                return;

            //
            // 로그인(인증서버)
            //

            string sAccessToken;
            if (!Login(out sAccessToken))
            {
                Console.WriteLine("Auth Login Failed");
                return;
            }

            //
            // 게임서버 접속
            //

            ServerPeer serverPeer = new ServerPeer();
            serverPeer.Start("192.168.0.44", 7000);

            GameServerSession session = new GameServerSession(serverPeer);
            session.SendLoginCommand(sAccessToken);

            //
            //
            //

            while (true)
            {
                serverPeer.Service();
                Thread.Sleep(100);
            }
        }

        //
        // 시스템정보
        //

        private static bool LoadSystemInfo()
        {
            string sUrl = "http://192.168.0.44:5000/Command.ashx?cmd=systemInfo";
            string sResponseText = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sUrl);
            request.Method = "GET";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                JsonData jsonData = JsonMapper.ToObject(reader);

                int nReturnCode = -1;
                if (!JsonUtil.GetParameter(jsonData, "returnCode", out nReturnCode))
                {
                    Console.WriteLine("Parsing Error.(returnCode)");
                    return false;
                }

                if (nReturnCode != 0)
                {
                    string sError = "";
                    if (!JsonUtil.GetParameter(jsonData, "error", out sError))
                    {
                        Console.WriteLine("Parsing Error.(error)");
                        return false;
                    }

                    Console.WriteLine("returnCode : " + nReturnCode + ", error : " + sError);
                    return false;
                }

                string sResponse = "";
                if (!JsonUtil.GetParameter(jsonData, "response", out sResponse))
                {
                    Console.WriteLine("Parsing Error.(response)");
                    return false;
                }

                JsonData jsonResponse = JsonMapper.ToObject(sResponse);
                Console.WriteLine("SystemInfo = " + jsonResponse);
            }

            return true;
        }

        //
        // 메타데이터
        //

        private static bool LoadMetaData()
        {
            string sMetaData = "";

            string sUrl = "http://192.168.0.44:5000/Command.ashx?cmd=metadata";
            string sResponseText = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sUrl);
            request.Method = "GET";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                JsonData jsonData = JsonMapper.ToObject(reader);

                int nReturnCode = -1;
                if (!JsonUtil.GetParameter(jsonData, "returnCode", out nReturnCode))
                {
                    Console.WriteLine("Parsing Error.(returnCode)");
                    return false;
                }

                if (nReturnCode != 0)
                {
                    string sError = "";
                    if (!JsonUtil.GetParameter(jsonData, "error", out sError))
                    {
                        Console.WriteLine("Parsing Error.(error)");
                        return false;
                    }

                    Console.WriteLine("returnCode : " + nReturnCode + ", error : " + sError);
                    return false;
                }

                string sResponse = "";
                if (!JsonUtil.GetParameter(jsonData, "response", out sResponse))
                {
                    Console.WriteLine("Parsing Error.(response)");
                    return false;
                }

                JsonData jsonResponse = JsonMapper.ToObject(sResponse);

                if (!JsonUtil.GetParameter(jsonResponse, "metaData", out sMetaData))
                {
                    Console.WriteLine("Parsing Error.(metaData)");
                    return false;
                }
            }

            Console.WriteLine(sMetaData);
            WriteMetaData(sMetaData);

            return true;
        }

        private static void WriteMetaData(string sMetaData)
        {
            string sPath = AppDomain.CurrentDomain.BaseDirectory + @"\Meta";
            DirectoryInfo di = new DirectoryInfo(sPath);

            if (!di.Exists)
                di.Create();

            string sFilePath = sPath + @"\metadata.txt";

            if (File.Exists(sFilePath))
                File.Delete(sFilePath);

            using (StreamWriter writer = File.CreateText(sFilePath))
            {
                writer.Write(sMetaData);
            }
        }

        //
        //
        //

        private static void ReadMovePosition()
        {
            string sData;

            string sPath = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo di = new DirectoryInfo(sPath);

            string sFilePath = sPath + @"\MovePosition.txt";

            if (!File.Exists(sFilePath))
                return;

            using (StreamReader reader = File.OpenText(sFilePath))
            {
                sData = reader.ReadToEnd();
            }

            string[] sVectors = sData.Split('\n');

            foreach (string sVector in sVectors)
			{
                string[] sElemental = sVector.Split(',');

                if (sElemental.Length == 3)
                {
                    Vector3 v = new Vector3(Convert.ToSingle(sElemental[0]), Convert.ToSingle(sElemental[1]), Convert.ToSingle(sElemental[2]));
                    movePosition.Add(v);
                }
            }
		}

		//
		// 로그인(인증서버)
		//

		private static bool Login(out string sAccessToken)
        {
            sAccessToken = "";
            Guid id = Guid.NewGuid();

            string sUrl = "http://192.168.0.44:5000/Command.ashx?cmd=login&id=" + id;
            string sResponseText = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sUrl);
            request.Method = "GET";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                JsonData jsonData = JsonMapper.ToObject(reader);

                int nReturnCode = -1;
                if (!JsonUtil.GetParameter(jsonData, "returnCode", out nReturnCode))
                {
                    Console.WriteLine("Parsing Error.(returnCode)");
                    return false;
                }

                if (nReturnCode != 0)
                {
                    string sError = "";
                    if (!JsonUtil.GetParameter(jsonData, "error", out sError))
                    {
                        Console.WriteLine("Parsing Error.(error)");
                        return false;
                    }

                    Console.WriteLine("returnCode : " + nReturnCode + ", error : " + sError);
                    return false;
                }

                if (!JsonUtil.GetParameter(jsonData, "response", out sAccessToken))
                {
                    Console.WriteLine("Parsing Error.(response)");
                    return false;
                }
            }

            return true;
        }
    }
}
