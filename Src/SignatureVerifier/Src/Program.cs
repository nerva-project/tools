using System;
using AngryWasp.Cli.Args;
using AngryWasp.Cryptography;
using AngryWasp.Logger;

namespace Tools
{
    public class MainClass
    {
        [STAThread]
        public static void Main(string[] rawArgs)
        {
            Arguments args = Arguments.Parse(rawArgs);
            Log.CreateInstance(true);

            if (args["in-file"] == null)
                Log.Instance.Write(Log_Severity.Fatal, "Need an input file");
            
            if (args["sig-file"] == null)
                Log.Instance.Write(Log_Severity.Fatal, "Need a signature file");

            string inFile = args["in-file"].Value;
            string sigFile = args["sig-file"].Value;

            string keyringTag, keyId;
            if (PgpVerifier.Verify(sigFile, inFile, out keyringTag, out keyId))
                Console.WriteLine(keyId);
            else
                Console.Write("File signature failed verification");
        }
    }
}