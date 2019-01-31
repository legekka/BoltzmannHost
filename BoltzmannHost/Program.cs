using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperWebSocket;
using Newtonsoft.Json;


namespace BoltzmannHost
{
    class Program
    {
        public static List<WebSocketSession> Sessions;
        private static WebSocketServer wsServer;
        static void Main(string[] args)
        {
            wsServer = new WebSocketServer();
            int port = 9880;
            wsServer.Setup(port);
            wsServer.NewSessionConnected += WsServer_NewSessionConnected;
            wsServer.NewMessageReceived += WsServer_NewMessageReceived;
            wsServer.NewDataReceived += WsServer_NewDataReceived;
            wsServer.SessionClosed += WsServer_SessionClosed;
            wsServer.Start();
            Console.WriteLine("Server is running on port " + port + ". Press any key to exit...");
            Console.ReadKey();
            wsServer.Stop();
        }
        private static void WsServer_NewSessionConnected(WebSocketSession session)
        {
            Console.WriteLine(session.Host + " NewSessionConnected " + session.Origin);
            Sessions.Add(session);
            
        }
        private static void WsServer_NewMessageReceived(WebSocketSession session, string value)
        {
            Console.WriteLine("NewMessageRecieved" + session.Host + " " + session.Origin + " " + value);
            if (value == "ezkelllegyen")
            {
                session.Send("Anyád");
            }
        }

        private static void WsServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            Console.WriteLine(session.Host + " session_closed " + session.Origin);
            int i = 0;
            while(i<Sessions.Count && Sessions[i].SessionID != session.SessionID) { i++; }
            if (i == Sessions.Count)
                return;
            Sessions.RemoveAt(i);
        }

        private static void WsServer_NewDataReceived(WebSocketSession session, byte[] value)
        {
            Console.WriteLine("NewDataRecieved " + session.Host + " " + session.Origin + " " + value.ToString());
        }

        public void SendFile(WebSocketSession session)
        {
            string message = "packet|";
            List<FilePacket> filePackets = FilePacker.Generate(@"E:\cpu0001.png", 32768);
            foreach (var packet in filePackets)
                session.Send(message + JsonConvert.SerializeObject(packet));
        }
    }
}
