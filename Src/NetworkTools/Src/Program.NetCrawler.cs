using System;
using AngryWasp.Logger;
using System.Collections.Generic;
using Nerva.Levin;
using AngryWasp.Cli.Args;

namespace Tools
{
    public static class MainClass_NetCrawler
    {
        private static Log log = new Log(true);

        [STAThread]
        public static void Main(string[] rawArgs)
        {
            Arguments args = Arguments.Parse(rawArgs);
            Log.CreateInstance(false);

            if (args["host"] == null)
               log.Write(Log_Severity.Fatal, "Need a host");
            
            string host = args["host"].Value;

            Crawler c = new Crawler(log);

            c.ProbeNode(host);

            log.SetColor(ConsoleColor.Yellow);
            Console.WriteLine($"Found {c.AllNodes.Count} nodes, verified {c.VerifiedNodes.Count}");
            log.SetColor(ConsoleColor.White);
        }
    }

    public class Crawler
    {
        private HashSet<string> verifiedNodes = new HashSet<string>();
        private HashSet<string> allNodes = new HashSet<string>();

        private Log log;

        public HashSet<string> AllNodes => allNodes;
        public HashSet<string> VerifiedNodes => verifiedNodes;

        public Crawler(Log log)
        {
            this.log = log;
        }

        public void ProbeNode(string host, int port = 17565)
        {
            NetworkConnection nc = new NetworkConnection();
            object pl = nc.Run(host, port);

            if (pl != null)
            {
                verifiedNodes.Add(host);
                log.Write($"{host.PadLeft(15)}");

                object[] sec = (object[])pl;
                foreach (var s in sec)
                {
                    Section entry = (Section)s;
                    Section adr = (Section)entry.Entries["adr"];
                    Section addr = (Section)adr.Entries["addr"];

                    uint ipInt = 0;
                    
                    if (addr.Entries.ContainsKey("m_ip"))
                        ipInt = (uint)addr.Entries["m_ip"];

                    if (ipInt == 0)
                        continue;

                    string ip = ToIP(ipInt);
                    
                    if (!allNodes.Contains(ip))
                    {
                        allNodes.Add(ip);
                        ProbeNode(ip);
                    }
                }
            }
        }

        public static string ToIP(uint ip)
        {
            return String.Format("{3}.{2}.{1}.{0}",
                ip >> 24, (ip >> 16) & 0xff, (ip >> 8) & 0xff, ip & 0xff);
        }
    }
}
