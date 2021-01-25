using System;
using AngryWasp.Cli.Args;
using AngryWasp.Logger;
using Log = AngryWasp.Logger.Log;

namespace Tools
{
    public class MainClass_NodeValidator
    {
        [STAThread]
        public static void Main(string[] rawArgs)
        {
            Arguments args = Arguments.Parse(rawArgs);
            Log.CreateInstance(true);

            if (args["host"] == null)
                Log.Instance.Write(Log_Severity.Fatal, "Need a host");
            
            string host = args["host"].Value;

            int port = 17565;
            if (args["testnet"] != null)
                port = 18565;

            NetworkConnection nc = new NetworkConnection();
            nc.Run(host, port);
        }
    }
}