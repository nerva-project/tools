using System;
using AngryWasp.Cryptography;
using AngryWasp.Helpers;
using AngryWasp.Logger;

namespace Tools
{
    public class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            CommandLineParser cmd = CommandLineParser.Parse(args);
            Log.CreateInstance(true);

            if (cmd["in-file"] == null)
                Log.Instance.Write(Log_Severity.Fatal, "Need an input file");
            
            if (cmd["sig-file"] == null)
                Log.Instance.Write(Log_Severity.Fatal, "Need a signature file");

            string inFile = cmd["in-file"].Value;
            string sigFile = cmd["sig-file"].Value;

            string keyringTag, keyId;
            if (PgpVerifier.Verify("./VerifyTestData/rand.bin.sig","./VerifyTestData/rand.bin", out keyringTag, out keyId))
                Console.WriteLine(keyId);
            else
                Console.Write("File signature failed verification");
        }
    }
}