using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Log = AngryWasp.Logger.Log;

namespace Tools
{
    public class MainClass_NodeValidator
    {
        [STAThread]
        public static void Main(string[] args)
        {
            CommandLineParser cmd = CommandLineParser.Parse(args);
            Log.CreateInstance(true);

            if (cmd["host"] == null)
                Log.Instance.Write(Log_Severity.Fatal, "Need a host");
            
            string host = cmd["host"].Value;

            int port = 17565;
            if (cmd["testnet"] != null)
                port = 18565;

            NetworkConnection nc = new NetworkConnection();
            nc.Run(host, port);
        }
    }
}