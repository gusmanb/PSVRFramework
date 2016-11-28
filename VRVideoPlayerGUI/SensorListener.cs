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
