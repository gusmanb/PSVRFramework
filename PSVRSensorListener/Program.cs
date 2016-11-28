/*
* PSVRFramework - PlayStation VR PC framework
* Copyright (C) 2016 Agustín Giménez Bernad <geniwab@gmail.com>
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as
* published by the Free Software Foundation, either version 3 of the
* License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSVRSensorListener
{
    class Program
    {
        static UdpClient client;
        static void Main(string[] args)
        {
            bool ip = false;
            bool pt = false;

            IPAddress address = IPAddress.Any;
            int port = 0;

            while (!pt)
            {
                Console.WriteLine("Enter the broadcast port");
                var po = Console.ReadLine();
                pt = int.TryParse(po, out port);

                if (!pt)
                {
                    ip = false;
                    Console.WriteLine("Invalid port");
                }
            }

            Console.WriteLine("Starting UDP client, press any key to exit");
            Thread.Sleep(1000);
            IPEndPoint ep = new IPEndPoint(address, port);

            client = new UdpClient();
            client.EnableBroadcast = true;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.ExclusiveAddressUse = false;
            client.Client.Bind(ep);

            Task.Run(() => 
            {
                try
                {
                    while (true)
                    {
                        byte[] data = client.Receive(ref ep);
                        string sData = Encoding.UTF8.GetString(data);
                        Console.WriteLine("--Begin update--");
                        Console.WriteLine(sData);
                        Console.WriteLine("--End update--");
                    }
                }
                catch { }
            });

            Console.ReadKey();
            client.Close();
        }
    }
}
