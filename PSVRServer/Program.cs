using PSVRFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PSVRServerTest
{
    class Program
    {
        static PSVRServer server;

        static void Main(string[] args)
        {
            server = new PSVRServer(IPAddress.Parse("127.0.0.1"), 9354);
            Console.WriteLine("Server running, press any key to stop");
            Console.ReadKey();
            server.Dispose();

        }
    }
}
