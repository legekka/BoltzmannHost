using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Threading;

namespace BoltzmannHost
{
    class wsServer
    {
        public static List<BSession> bSessions;
        public static WebSocketServer ws;

        public wsServer(int port)
        {
            ws = new WebSocketServer();
            ws.Setup(port);
            ws.NewSessionConnected += WsServer_NewSessionConnected;
            ws.NewMessageReceived += WsServer_NewMessageReceived;
            ws.NewDataReceived += WsServer_NewDataReceived;
            ws.SessionClosed += WsServer_SessionClosed;
            ws.Start();
            bSessions = new List<BSession>();
            Console.WriteLine("Server is running on port " + port + ". Press any key to exit...");
            Console.ReadKey();
            ws.Stop();
        }

        private static void WsServer_NewSessionConnected(WebSocketSession session)
        {
            bSessions.Add(new BSession(session));
            int index = getIndex(session);
            bSessions[index].SetWorkingFolder(index);
            Console.WriteLine("WSC_" + index + " connected.");
            RequestClientInfo(session);
        }
        private static void WsServer_NewMessageReceived(WebSocketSession session, string value)
        {
            int index = getIndex(session);
            string msg = value.Split('|')[0];
            switch (msg)
            {
                case "clientinfo":
                    {
                        bSessions[index].ClientInfo = JsonConvert.DeserializeObject<ClientInfo>(value.Split('|')[1]);
                        Console.WriteLine("Got ClientInfo{WSC_" + index + "}");
                    }
                    break;

                case "packet":
                    {
                        FilePacket filePacket = JsonConvert.DeserializeObject<FilePacket>(value.Split('|')[1]); 
                        Console.WriteLine("Got Packet{" + filePacket.PacketID + "}");

                        bSessions[index].FilePackets.Add(filePacket);
                        if (filePacket.LastPacket)
                        {
                            if (CheckFilePackets(filePacket.PacketID, index))
                            {
                                FilePacker.BuildFile(bSessions[index].FilePackets, bSessions[index].WorkingFolder);
                                Console.WriteLine("File built. " + bSessions[index].WorkingFolder + @"\" + filePacket.FileName);
                            }
                        }
                    }
                    break;
                case "result":
                    {
                        bSessions[index].Result = JsonConvert.DeserializeObject<Result>(value.Split('|')[1]);
                        Console.WriteLine("Got Result{WSC_" + index + ":" + bSessions[index].Result.JobID + "}");
                        Console.WriteLine(bSessions[index].Result.RenderSetting.OutputPath);
                    }
                    break;
            }
        }

        private static void WsServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            int index = getIndex(session);
            Console.WriteLine("WSC_" + index + " session closed.");
            bSessions.RemoveAt(index);
        }

        private static void WsServer_NewDataReceived(WebSocketSession session, byte[] value)
        {
            Console.WriteLine("NewDataRecieved " + session.Host + " " + session.Origin + " " + value.ToString());
        }

        public static void RequestClientInfo(WebSocketSession session)
        {
            session.Send("req clientinfo|");
        }

        public static void SendFile(WebSocketSession session)
        {
            int index = getIndex(session);
            int sample = CalculateSample(getIndex(session), 250);
            RenderSetting renderSetting = new RenderSetting("teszt.blend", "kep", sample, index);
            Job job = new Job("teszt01", renderSetting);
            string message = "new job|";
            session.Send(message + JsonConvert.SerializeObject(job));


            message = "packet|";
            List<FilePacket> filePackets = FilePacker.Generate(@"H:\teszt.blend", 32768);
            foreach (var packet in filePackets)
                session.Send(message + JsonConvert.SerializeObject(packet));
        }

        private static int CalculateSample(int index, int sample)
        {
            int sum = 0;
            int perf;
            if (bSessions[index].ClientInfo.useGPU)
                perf = bSessions[index].ClientInfo.GpuScore;
            else
                perf = bSessions[index].ClientInfo.CpuScore;
            foreach (var bSession in bSessions)
                if (bSession.ClientInfo.useGPU)
                    sum += bSession.ClientInfo.GpuScore;
                else
                    sum += bSession.ClientInfo.CpuScore;
            Console.WriteLine("Total performance: " + sum);
            Console.WriteLine("WSC_" + index + " performance: " + perf);
            double ratio = (double)perf / (double)sum;
            Console.WriteLine("Render ratio: " + ratio);
            int calculated = (int)Math.Round(sample * ratio);
            Console.WriteLine("Calculated Sample: " + calculated);
            return calculated;
        }

        private static int getIndex(WebSocketSession session)
        {
            int i = 0;
            while (i < bSessions.Count && session.SessionID != bSessions[i].Session.SessionID) { i++; }
            if (i == bSessions.Count)
                throw new IndexOutOfRangeException();

            return i;
        }

        private static bool CheckFilePackets(int n, int index)
        {
            int i = 0;
            while (i < n && bSessions[index].FilePackets[i] != null) { i++; }
            return (i == n);
        }
    }
}
