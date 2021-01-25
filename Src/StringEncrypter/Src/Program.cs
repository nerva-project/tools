using System;
using System.IO;
using System.Security.Cryptography;
using AngryWasp.Cli;
using AngryWasp.Cli.Args;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Log = AngryWasp.Logger.Log;

namespace Tools
{
    public class MainClass
    {
        [STAThread]
        public static void Main(string[] rawArgs)
        {
            Arguments args = Arguments.Parse(rawArgs);
            Log.CreateInstance(true);

            if (args["random"] != null)
            {
                var count = args.GetInt("random", 10);

                for (int i = 0; i < count; i++)
                {
                    string text = StringHelper.GenerateRandomString(32);
                    string key = StringHelper.GenerateRandomString(32);
                    byte[] b64 = Convert.FromBase64String(text.Encrypt(key));
                    byte[] result = Reduce(Reduce(SHA256.Create().ComputeHash(b64)));
                    Log.Instance.Write(Log_Severity.None, result.ToHex());
                }
                
                return;
            }

            string pw = null;

            if (args["password"] != null)
                pw = args["password"].Value;
            else
            {
                pw = PasswordPrompt.Get();
                string pw2 = PasswordPrompt.Get("Please confirm your password");

                if (pw != pw2)
                    Log.Instance.Write(Log_Severity.Fatal, "Passwords did not match");
            }

            if (args["encrypt"] != null)
            {
                string text = args["encrypt"].Value;
                if (File.Exists(text))
                    text = File.ReadAllText(text);
                
                try {
                    Log.Instance.Write(Log_Severity.None, text.Encrypt(pw));
                } catch {
                    Log.Instance.Write(Log_Severity.Fatal, "Encryption failed");
                }
            }

            if (args["decrypt"] != null)
            {
                string text = args["decrypt"].Value;
                if (File.Exists(text))
                    text = File.ReadAllText(text);

                try {
                    Log.Instance.Write(Log_Severity.None, text.Decrypt(pw));
                } catch {
                    Log.Instance.Write(Log_Severity.Fatal, "Decryption failed");
                }
            }
        }

        private static byte[] Reduce(byte[] input)
        {
            byte[] output = new byte[input.Length / 2];
            int x = 0;
            for (int i = 0; i < input.Length; i += 2)
                output[x++] = (byte)(input[i] ^ input[i + 1]);

            return output;
        }
    }
}