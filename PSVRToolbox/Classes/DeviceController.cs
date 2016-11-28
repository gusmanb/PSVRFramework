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

using PSVRFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSVRToolbox.Classes
{
    public class DeviceController : IDisposable
    {
        string serial = "";
        bool local = false;
        bool disposed = false;
        CancellationTokenSource cancel;

        public event EventHandler<StatusEventArgs> DeviceStatusChanged;
        public event EventHandler<InputEventArgs> InputUpdate;

        PSVRServer server;
        PSVRClient client;
        IPAddress address;
        int port;

        public DeviceController(IPAddress ServerAddress, int Port)
        {
            cancel = new CancellationTokenSource();
            address = ServerAddress;
            port = Port;

            if (ServerAddress == null)
            {
                local = true;
                server = new PSVRServer(IPAddress.Parse("127.0.0.1"), Port);
                StartServerConnection(cancel.Token);
            }
            else
            {
                local = false;
                StartServerConnection(cancel.Token);
            }
        }

        private async void StartServerConnection(CancellationToken Cancellation)
        {
            try
            {
                while (!Cancellation.IsCancellationRequested)
                {
                    if (client != null)
                        await Task.Delay(1000, Cancellation);
                    else
                    {
                        try
                        {
                            client = new PSVRClient(address, port);
                            client.Closed += Client_Closed;
                            client.InputUpdate += Client_InputUpdate;
                            client.StatusUpdate += Client_StatusUpdate;
                            await client.RequestDeviceState();
                        }
                        catch { await Task.Delay(1000, Cancellation); }
                    }
                }

            }catch { }
        }

        private void Client_StatusUpdate(object sender, StatusEventArgs e)
        {
            if (e.Connected)
            {
                serial = e.SerialNumber;
                if (DeviceStatusChanged != null)
                    DeviceStatusChanged(this, e);
            }
            else if(serial != null)
            {
                serial = null;

                if (DeviceStatusChanged != null)
                    DeviceStatusChanged(this, e);
            }
        }

        private void Client_InputUpdate(object sender, InputEventArgs e)
        {
            if (InputUpdate != null)
                InputUpdate(this, e);
        }

        private void Client_Closed(object sender, EventArgs e)
        {
            client = null;

            if (DeviceStatusChanged != null)
                DeviceStatusChanged(this, new StatusEventArgs { Connected = false, SerialNumber = serial ?? "" });

        }

        public async Task<bool> HeadsetOn()
        {
            if (client == null)
                return false;

            return await client.HeadsetOn();
        }

        public async Task<bool> HeadsetOff()
        {

            if (client == null)
                return false;

            return await client.HeadsetOff();

        }

        public async Task<bool> EnableVRTracking()
        {

            if (client == null)
                return false;

            return await client.EnableVRTracking();

        }

        public async Task<bool> EnableVRMode()
        {

            if (client == null)
                return false;

            return await client.EnableVRMode();

        }

        public async Task<bool> EnableCinematicMode()
        {

            if (client == null)
                return false;

            return await client.EnableCinematicMode();
        }
        
        public async Task<bool> Shutdown()
        {

            if (client == null)
                return false;

            return await client.Shutdown();

        }

        public async Task<bool> LedsOn()
        {

            if (client == null)
                return false;

            return await client.LedsOn();

        }

        public async Task<bool> LedsOff()
        {

            if (client == null)
                return false;

            return await client.LedsOff();

        }

        public async Task<bool> LedsDefault()
        {
            if (client == null)
                return false;

            return await client.LedsDefault();

        }

        public async Task<bool> RequestDeviceState()
        {

            if (client == null)
                return false;

            return await client.RequestDeviceState();

        }

        public async Task<bool> ResetPose()
        {

            if (client == null)
                return false;

            return await client.ResetPose();

        }

        public async Task<bool> RecalibrateDevice()
        {

            if (client == null)
                return false;

            return await client.RecalibrateDevice();

        }

        public async Task<bool> ApplyCinematicSettings(byte ScreenDistance, byte ScreenSize, byte Brightness, byte MicFeedback)
        {
            if (client == null)
                return false;

            return await client.ApplyCinematicSettings(ScreenDistance, ScreenSize, Brightness, MicFeedback);

        }

        public async Task<bool> ApplyLedSettings(byte[] Values)
        {

            if (client == null)
                return false;

            return await client.ApplyLedSettings(Values);

        }
        
        public void Dispose()
        {
            cancel.Cancel();
            cancel.Dispose();
            cancel = null;

            if (client != null)
            {
                client.Dispose();
                client = null;
            }
            if (server != null)
            {
                server.Dispose();
                server = null;
            }

            DeviceStatusChanged = null;
            InputUpdate = null;
        }
    }
}
