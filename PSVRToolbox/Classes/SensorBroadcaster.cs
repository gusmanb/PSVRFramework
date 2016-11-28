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
using System.Threading.Tasks;
using Newtonsoft.Json;
using PSVRFramework;

namespace PSVRToolbox
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
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SensorData));
            client.Send(data, data.Length, ep);
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
