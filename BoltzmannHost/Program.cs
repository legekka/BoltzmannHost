using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperWebSocket;


namespace BoltzmannHost
{
    class Program
    {
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

        private static void WsServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            Console.WriteLine(session.Host + " session_closed " + session.Origin);
        }

        private static void WsServer_NewDataReceived(WebSocketSession session, byte[] value)
        {
            Console.WriteLine("NewDataRecieved " + session.Host + " " + session.Origin + " " + value.ToString());
        }

        private static void WsServer_NewMessageReceived(WebSocketSession session, string value)
        {
            Console.WriteLine("NewMessageRecieved" + session.Host + " " + session.Origin + " " + value);
            if (value == "ezkelllegyen")
            {
                session.Send("Anyád");
            }
        }

        private static void WsServer_NewSessionConnected(WebSocketSession session)
        {
            Console.WriteLine(session.Host + " NewSessionConnected " + session.Origin);
        }
    }
}
