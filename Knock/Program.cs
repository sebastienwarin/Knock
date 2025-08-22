/*
 *  A Windows port knocking client
 *  Web site: http://sebastien.warin.fr
 *  Copyright (C) 2016 - Sebastien Warin <http://sebastien.warin.fr>	   	  
 *	
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 * 
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

namespace Knock
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;

    class Program
    {
        private static readonly byte[] packetData = new byte[] { 0x00, 0x23, 0xf8, 0x4e, 0x45, 0x7e, 0xf4, 0x6d, 0x04, 0x39, 0xd2, 0xeb, 0x08, 0x00, 0x45, 0x00, 0x00, 0x34, 0x13, 0xea, 0x40, 0x00, 0x80, 0x06, 0x61, 0x19, 0xc0, 0xa8, 0x01, 0x22, 0x5d, 0x59, 0x66, 0x9d, 0x65, 0x48, 0x00, 0x50, 0x05, 0xe6, 0x36, 0xbb, 0x00, 0x00, 0x00, 0x00, 0x80, 0x02, 0x20, 0x00, 0x27, 0xde, 0x00, 0x00, 0x02, 0x04, 0x04, 0xec, 0x01, 0x03, 0x03, 0x08, 0x01, 0x01, 0x04, 0x02 };
        
        // Try to parse as IP, otherwise resolve as domain name
        static bool TryGetAddress(string input, out IPAddress address)
        {
            address = null;
            if (string.IsNullOrWhiteSpace(input))
                return false;
            if (IPAddress.TryParse(input, out address))
                return true;
            try
            {
                var addresses = Dns.GetHostAddresses(input);
                address = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                return address != null;
            }
            catch
            {
                return false;
            }
        }

        static void Main(string[] args)
        {
            IPAddress address = null;
            bool useTcp = args.Contains("--tcp");
            var portArgs = args.Where(a => a != "--tcp").ToArray();

            if (portArgs.Length >= 2)
            {
                if (!TryGetAddress(portArgs[0], out address))
                {
                    Console.WriteLine($"Could not resolve IP or domain: {portArgs[0]}");
                    return;
                }
                if (!IPAddress.TryParse(portArgs[0], out _))
                    Console.WriteLine($"Resolved {portArgs[0]} to {address}");

                foreach (var arg in portArgs.Skip(1))
                {
                    int port = 0;
                    if (int.TryParse(arg, out port))
                    {
                        if (useTcp)
                        {
                            Console.WriteLine($"Sending TCP SYN to {address}:{port}");
                            try
                            {
                                using (var tcpClient = new System.Net.Sockets.TcpClient())
                                {
                                    var result = tcpClient.BeginConnect(address.ToString(), port, null, null);
                                    bool success = result.AsyncWaitHandle.WaitOne(500); // 500ms timeout
                                    tcpClient.Close(); // Close immediately after SYN
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"TCP SYN to {address}:{port} failed: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Sending UDP packet to {address}:{port}");
                            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                            socket.SendTo(packetData, new IPEndPoint(address, port));
                            socket.Close();
                        }
                    }
                }
                Console.WriteLine("Done");
            }
            else
            {
                Console.WriteLine("usage: knock.exe <IP|domain> <port> [port] ... [--tcp]");
                Console.WriteLine("example default UDP knock: knock.exe 192.168.0.1 123 456 789");
                Console.WriteLine("example tcp SYN: knock.exe example.com 123 456 789 --tcp");
            }
        }        
    }
}
