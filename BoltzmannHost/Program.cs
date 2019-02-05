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
        public static wsServer wsServer;

        static void Main(string[] args)
        {
            wsServer = new wsServer(9880);
        }
        
    }
}
