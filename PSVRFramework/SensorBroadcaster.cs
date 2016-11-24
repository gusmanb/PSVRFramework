using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PSVRFramework
{
    public class SensorBroadcaster : IDisposable
    {
        UdpClient client;
        IPEndPoint ep;
        public SensorBroadcaster(string BroadcastAddress, int Port)
        {
            IPAddress address = IPAddress.Parse(BroadcastAddress);
            ep = new IPEndPoint(address, Port);
            client = new UdpClient();
            client.EnableBroadcast = true;
        }

        public void Broadcast(PSVRSensorReport SensorData)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SensorData));
                client.Send(data, data.Length, ep);
            }
            catch { }
        }

        public void Dispose()
        {
            client.Close();
        }
    }

    public class OpenTrackSender : IDisposable
    {
        UdpClient client;
        IPEndPoint ep;
        public OpenTrackSender(int Port)
        {
            IPAddress address = IPAddress.Any;
            ep = new IPEndPoint(address, Port);
            client = new UdpClient();
            client.EnableBroadcast = true;
        }

        public void Broadcast(PSVRSensorReport SensorData)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SensorData));
            client.Send(data, data.Length, ep);
        }

        public void Dispose()
        {
            client.Close();
        }
    }
}
