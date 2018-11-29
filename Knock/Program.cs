/*
 *  A Windows port knocking client
 *  Web site: http://sebastien.warin.fr
 *  Copyright (C) 2016 - Sebastien Warin <http://sebastien.warin.fr>	   	 
 *  TCP Support added by Josh Jameson <josh@servebyte.com>
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
    using System.Threading.Tasks;

    class Program
    {
        private static readonly byte[] packetData = new byte[] { 0x00, 0x23, 0xf8, 0x4e, 0x45, 0x7e, 0xf4, 0x6d, 0x04, 0x39, 0xd2, 0xeb, 0x08, 0x00, 0x45, 0x00, 0x00, 0x34, 0x13, 0xea, 0x40, 0x00, 0x80, 0x06, 0x61, 0x19, 0xc0, 0xa8, 0x01, 0x22, 0x5d, 0x59, 0x66, 0x9d, 0x65, 0x48, 0x00, 0x50, 0x05, 0xe6, 0x36, 0xbb, 0x00, 0x00, 0x00, 0x00, 0x80, 0x02, 0x20, 0x00, 0x27, 0xde, 0x00, 0x00, 0x02, 0x04, 0x04, 0xec, 0x01, 0x03, 0x03, 0x08, 0x01, 0x01, 0x04, 0x02 };

        static void Main(string[] args)
        {
            IPAddress address = null;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (args.Length >= 3 && (args[0] == "UDP"|| args[0] == "TCP") && IPAddress.TryParse(args[1], out address))
            {
                foreach (var arg in args.Skip(2))
                {
                    int port = 0;
                    if (int.TryParse(arg, out port))
                    {
                        if (args[0] == "TCP")
                        {
                            Console.WriteLine($"Connecting to TCP {address}:{port}");
                            var tcpClient = new TcpClient();
                            Task connectTask = tcpClient.ConnectAsync(address, port);
                            tcpClient.Close();
                            System.Threading.Thread.Sleep(500);
                        } else
                        if (args[0] == "UDP")
                        {
                            Console.WriteLine($"Sending UDP packet to {address}:{port}");
                            socket.SendTo(packetData, new IPEndPoint(address, port));
                        }
                    }
                }
                Console.WriteLine("Done");
            }
            else
            {
                Console.WriteLine("usage: knock.exe <TCP|UDP> <IP> <port> [port] ...");
                Console.WriteLine("example: knock.exe UDP 192.168.0.1 123 456 789");
            }
        }
    }
}
