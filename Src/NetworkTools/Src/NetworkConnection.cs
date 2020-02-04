using System;
using System.Net.Sockets;
using System.Threading;
using AngryWasp.Logger;
using Nerva.Levin;
using Nerva.Levin.Requests;

namespace Tools
{
    public class NetworkConnection
    {
        public object Run(string host, int port = 17565)
        {
            Log.Instance.Write($"Scanning {host}:{port}");
            TcpClient client;

            if (!Ping(host, out client, port))
            {
                Log.Instance.Write(Log_Severity.Error, "Ping: Failed");
                return null;
            }
            else
                Log.Instance.Write("Ping: OK");
                
            object pl = VerifyPing(host, client);
            
            if (pl == null)
            {
                Log.Instance.Write(Log_Severity.Error, "Verify: Failed");
                return null;
            }
            else
                Log.Instance.Write("Verify: OK");
            
            return pl;
        }

        public bool Ping(string host, out TcpClient client, int port = 17565, int timeout = 1500)
        {
            try
            {
                client = new TcpClient();
                IAsyncResult result = client.BeginConnect(host, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
                if (!success)
                {
                    client = null;
                    return false;
                }

                return true;
            }
            catch
            {
                client = null;
                return false;
            }
        }

        public object VerifyPing(string host, TcpClient client)
        {
            Log.Instance.Write($"{host}: Validating");

            try
            {
                NetworkStream ns = client.GetStream();

                object peerlist = null;

                byte[] handshake = new Handshake().Create();
                Log.Instance.Write($"{host}: Sending handshake packet");

                ns.Write(handshake, 0, handshake.Length);

                Header header; Section section;
                while (Read(host, ns, out header, out section))
                {
                    if (header.Command == Constants.P2P_COMMAND_HANDSHAKE)
                    {
                        peerlist = section.Entries["local_peerlist_new"];
                        Log.Instance.Write("Retrieved peer list. Disconnecting");
                        break;
                    }
                }

                ns.Close();
                client.Close();

                return peerlist;
            }
            catch (Exception ex)
            {
                Log.Instance.WriteNonFatalException(ex);
                if (client != null)
                    client.Close();
            }

            return null;
        }

        public bool ReadNetworkStream(NetworkStream networkStream, int expectedLength, out byte[] buffer)
        {
            buffer = new byte[expectedLength];
            int x = 0, y = 0;

            int emptyReadCount = 0;

            Thread.Sleep(250);

            do
            {
                try
                {
                    y = networkStream.Read(buffer, x, buffer.Length);
                    x += y;

                    if (y > 0)
                        emptyReadCount = 0;
                    else
                        emptyReadCount++;

                    if (emptyReadCount >= 20)
                    {
                        Log.Instance.Write(Log_Severity.Error, $"Read timout. Expected {expectedLength} bytes, got {x}");
                        break;
                    }

                    Thread.Sleep(250);
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteNonFatalException(ex);
                    Log.Instance.Write(Log_Severity.Error, $"Read exception. Expected {expectedLength} bytes, got {x}");
                    return false;
                }

            } while (x < buffer.Length);

            return x >= buffer.Length;
        }

        private bool Read(string host, NetworkStream ns, out Header header, out Section section)
        {
            header = null;
            section = null;
            byte[] buffer;
            int offset = 0;

            if (!ReadNetworkStream(ns, 33, out buffer))
                return false;

            header = Header.FromBytes(buffer, ref offset);
            offset = 0;

            if (header == null)
            {
                Log.Instance.Write(Log_Severity.Error, $"{host}: Invalid header remote node");
                return false;
            }

            if (!ReadNetworkStream(ns, (int)header.Cb, out buffer))
                return false;

            switch (header.Command)
            {
                case Constants.P2P_COMMAND_HANDSHAKE:
                    section = new Handshake().Read(header, buffer, ref offset);
                    break;
                case Constants.P2P_COMMAND_REQUEST_SUPPORT_FLAGS:
                    section = new SupportFlags().Read(header, buffer, ref offset);
                    break;
                default:
                    if (header.Command >= 2001 && header.Command <= 2009)
                        Log.Instance.Write($"Unsupported protocol notification {header.Command}");
                    else
                        Log.Instance.Write(Log_Severity.Warning, $"Command {header.Command} is not yet supported");
                    return false;
            }

            Log.Instance.Write($"{host}: Read data package {header.Command}");

            return true;
        }
    }
}
