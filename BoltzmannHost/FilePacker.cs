using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BoltzmannHost
{
    class FilePacker
    {
        public List<FilePacket> filePackets = new List<FilePacket>();


        public FilePacker() { }

        public static List<FilePacket> Generate(string filePath, int packetSize)
        {
            List<FilePacket> filePackets = new List<FilePacket>();

            if (!File.Exists(filePath))
                throw new FileNotFoundException();
            byte[] data = File.ReadAllBytes(filePath);
            int n = (int)Math.Ceiling((decimal)data.Length / packetSize);
            byte[] part = new byte[packetSize];
            FilePacket fPacket;

            for (int i = 0; i < n - 1; i++)
            {
                part = new byte[packetSize];
                Array.Copy(data, i * packetSize, part, 0, packetSize);
                fPacket = new FilePacket(i, Path.GetFileName(filePath), part);
                filePackets.Add(fPacket);
            }
            part = new byte[data.Length - ((n - 1) * packetSize)];
            Array.Copy(data, (n - 1) * packetSize, part, 0, data.Length - ((n - 1) * packetSize));
            fPacket = new FilePacket((n - 1), Path.GetFileName(filePath), part, true);
            filePackets.Add(fPacket);

            return filePackets;
        }

        public static void BuildFile(List<FilePacket> filePackets, string path)
        {
            List<byte> data = new List<byte>();
            for (int i = 0; i < filePackets.Count; i++)
            {
                data.AddRange(filePackets[i].Data);
            }
            File.WriteAllBytes(path + filePackets[0].FileName, data.ToArray());
            Console.ReadLine();
        }
    }
    class FilePacket
    {
        public int PacketID;
        public int TotalCount;
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
}
