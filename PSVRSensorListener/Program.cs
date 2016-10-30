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
