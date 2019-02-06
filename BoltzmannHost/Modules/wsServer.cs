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
            bSessions = new List<BSession>();
        }

        public void StartServer()
        {
            bSessions = new List<BSession>();
            ws.Start();
            Program.Form1.SetInfoText("Server started.");
        }
        public void StopServer()
        {
            ws.Stop();
            Program.Form1.SetInfoText("Server stopped.");
            ws.Dispose();
        }

        private static void WsServer_NewSessionConnected(WebSocketSession session)
        {
            bSessions.Add(new BSession(session));
            int index = getIndex(session);
            bSessions[index].SetWorkingFolder(index);
            Program.Form1.SetInfoText("WSC_" + index + " connected.");
            RequestClientInfo(session);
            UpdateListBox();

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
                        UpdateListBox();
                    }
                    break;

                case "packet":
                    {
                        FilePacket filePacket = JsonConvert.DeserializeObject<FilePacket>(value.Split('|')[1]); 

                        if (bSessions[index].FilePackets == null)
                        {
                            bSessions[index].FilePackets = new List<FilePacket>();
                        }
                        bSessions[index].FilePackets.Add(filePacket);
                        if (filePacket.LastPacket)
                        {
                            if (CheckFilePackets(filePacket.PacketID, index))
                            {
                                FilePacker.BuildFile(bSessions[index].FilePackets, bSessions[index].WorkingFolder);
                                bSessions[index].FilePackets = null;
                                Program.Form1.SetInfoText("WSC_" + index + ": File built. " + bSessions[index].WorkingFolder + @"\" + filePacket.FileName);
                            }
                        }
                    }
                    break;
                case "result":
                    {
                        bSessions[index].Result = JsonConvert.DeserializeObject<Result>(value.Split('|')[1]);
                        Program.Form1.SetInfoText("Got Result{WSC_" + index + ":" + bSessions[index].Result.JobID + "}");
                    }
                    break;
            }
        }

        private static void WsServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            int index = getIndex(session);
            Program.Form1.SetInfoText("WSC_" + index + " session closed.");
            bSessions.RemoveAt(index);
            UpdateListBox();
        }

        private static void WsServer_NewDataReceived(WebSocketSession session, byte[] value)
        {
            Program.Form1.SetInfoText("NewDataRecieved " + session.Host + " " + session.Origin + " " + value.ToString());
        }

        public static void RequestClientInfo(WebSocketSession session)
        {
            session.Send("req clientinfo|");
        }

        public static void SendFile(WebSocketSession session)
        {
            int index = getIndex(session);
            int sample = CalculateSample(bSessions[index], 250);
            RenderSetting renderSetting = new RenderSetting("teszt.blend", "kep", sample, index);
            Job job = new Job("teszt01", renderSetting);
            string message = "new job|";
            session.Send(message + JsonConvert.SerializeObject(job));


            message = "packet|";
            List<FilePacket> filePackets = FilePacker.Generate(@"H:\teszt.blend", 32768);
            foreach (var packet in filePackets)
                session.Send(message + JsonConvert.SerializeObject(packet));
        }

        private static int CalculateSample(BSession bsession, int sample)
        {
            /*int sum = 0;
            int perf;
            if (bsession.ClientInfo.useGPU)
                perf = bsession.ClientInfo.GpuScore;
            else
                perf = bsession.ClientInfo.CpuScore;
            foreach (var bSession in bSessions)
                if (bSession.ClientInfo.useGPU)
                    sum += bSession.ClientInfo.GpuScore;
                else
                    sum += bSession.ClientInfo.CpuScore;
            double ratio = (double)perf / (double)sum;
            int calculated = (int)Math.Round(sample * ratio);
            return calculated;
            */
            return (int)Math.Round((double)sample / (double)bSessions.Count);
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

        public static async void UpdateListBox()
        {
            Program.Form1.ListBoxClear();
            int perf = 0;
            foreach (var bSession in bSessions)
            {
                if (bSession.ClientInfo.useGPU)
                    perf = bSession.ClientInfo.GpuScore;
                else
                    perf = bSession.ClientInfo.CpuScore;
                Program.Form1.AddListBoxElement("WSC_" + getIndex(bSession.Session) + " - " + perf);
            }
            
        }

        public static void SendJobs(string filepath, int sample)
        {
            RenderSetting renderSetting;
            Job job;
            string filename = Path.GetFileName(filepath);
            string filenameshort = Path.GetFileNameWithoutExtension(filepath);
            string message;
            
            for (int i = 0; i < bSessions.Count; i++)
            {
                renderSetting = new RenderSetting(filename, filenameshort, CalculateSample(bSessions[i], sample), i);
                job = new Job(filenameshort, renderSetting);

                message = "new job|";
                bSessions[i].Session.Send(message + JsonConvert.SerializeObject(job));

                message = "packet|";
                List<FilePacket> filePackets = FilePacker.Generate(filepath, 32768);
                foreach (var packet in filePackets)
                    bSessions[i].Session.Send(message + JsonConvert.SerializeObject(packet));
            }
        }
    }
}
