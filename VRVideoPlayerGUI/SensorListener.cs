using Newtonsoft.Json;
using PSVRFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VRVideoPlayerGUI
{
    class SensorListener : IDisposable
    {
        UdpClient client;

        public event EventHandler<SensorEventArgs> DataReceived;

        public SensorListener()
        {
            bool ip = false;
            bool pt = false;

            IPAddress address = IPAddress.Any;
            int port = 9090;
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
                        PSVRSensorReport report = JsonConvert.DeserializeObject<PSVRSensorReport>(sData);

                        if (report != null && DataReceived != null)
                            DataReceived(this, new SensorEventArgs { SensorReport = report });
                    }
                }
                catch { }
            });

        }
        
        public void Dispose()
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }
        }

    }

    public class SensorEventArgs : EventArgs
    {
        public PSVRSensorReport SensorReport { get; set; }
    }
}
