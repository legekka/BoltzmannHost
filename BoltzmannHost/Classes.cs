using System.Collections.Generic;
using System.IO;
using SuperWebSocket;
using Newtonsoft.Json;

namespace BoltzmannHost
{
    class BSession
    {
        public WebSocketSession Session;
        public ClientInfo ClientInfo;
        public List<FilePacket> FilePackets;
        public Result Result;
        public string WorkingFolder;


        public BSession(WebSocketSession session)
        {
            Session = session;
            ClientInfo = new ClientInfo();
            FilePackets = new List<FilePacket>();
            Result = new Result();
            WorkingFolder = "";
        }

        public void SetWorkingFolder(int index)
        {
            WorkingFolder = "WSC_" + index;
            if (!Directory.Exists(WorkingFolder))
                Directory.CreateDirectory(WorkingFolder);
        }
    }

    class ClientInfo
    {
        public int CpuScore;
        public int GpuScore;
        public bool useGPU;

        public ClientInfo()
        {
            CpuScore = new int();
            GpuScore = new int();
            useGPU = new bool();
        }

        [JsonConstructor]
        public ClientInfo(int cpuscore, int gpuscore, bool usegpu)
        {
            CpuScore = cpuscore;
            GpuScore = gpuscore;
            useGPU = usegpu;
        }
    }

    class FilePacket
    {
        public int PacketID;
        public string FileName;
        public byte[] Data;
        public bool LastPacket;

        public FilePacket(int packetId, string fileName, byte[] data)
        {
            PacketID = packetId;
            FileName = fileName;
            Data = data;
            LastPacket = false;
        }
        [JsonConstructor]
        public FilePacket(int packetId, string fileName, byte[] data, bool lastPacket)
        {
            PacketID = packetId;
            FileName = fileName;
            Data = data;
            LastPacket = lastPacket;
        }
    }

    class Job
    {
        public string JobID;
        public RenderSetting RenderSetting;
        public bool hasResources;

        [JsonConstructor]
        public Job(string jobId, RenderSetting renderSetting)
        {
            JobID = jobId;
            RenderSetting = renderSetting;
            hasResources = false;
        }
    }

    class RenderSetting
    {
        public string FileName;
        public string OutputPath;
        public int Sample;
        public int Seed;
        public string CustomCommands;

        public RenderSetting(string filename, string outputpath, int sample, int seed)
        {
            FileName = filename;
            OutputPath = outputpath;
            Sample = sample;
            Seed = seed;
        }

        [JsonConstructor]
        public RenderSetting(string filename, string outputpath, int sample, int seed, string customcommands)
        {
            FileName = filename;
            OutputPath = outputpath;
            Sample = sample;
            Seed = seed;
            CustomCommands = customcommands;
        }
    }

    class Result
    {
        public string JobID;
        public RenderSetting RenderSetting;

        public Result()
        {
            JobID = null;
            RenderSetting = null;
        }

        [JsonConstructor]
        public Result(string jobid, RenderSetting rendersetting)
        {
            JobID = jobid;
            RenderSetting = rendersetting;
        }
    }
}
