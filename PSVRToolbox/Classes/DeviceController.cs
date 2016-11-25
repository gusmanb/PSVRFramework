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

        PSVRDevice device;
        PSVRClient client;
        IPAddress address;
        int port;

        public DeviceController(IPAddress ServerAddress, int Port = 0)
        {
            cancel = new CancellationTokenSource();

            if (ServerAddress == null)
            {
                local = true;
                StartDeviceDetect(cancel.Token);
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
        }

        private async void StartDeviceDetect(CancellationToken Cancel)
        {
            try
            {
                while (!Cancel.IsCancellationRequested)
                {
                    if (device != null)
                        await Task.Delay(1000, Cancel);
                    else
                    {
                        device = PSVRDevice.GetDevice();

                        if (device == null)
                            await Task.Delay(1000);
                        else
                        {
                            device.INReport += Device_INReport;
                            device.Removed += Device_Removed;
                            device.Controller.RequestDeviceInfo();
                        }

                    }
                }
            }
            catch { }
        }

        private void Device_Removed(object sender, EventArgs e)
        {
            device = null;
            
            if (DeviceStatusChanged != null)
                DeviceStatusChanged(this, new StatusEventArgs { Connected = false, SerialNumber = serial });

            serial = null;
        }

        private void Device_INReport(object sender, PSVRINEventArgs e)
        {
            if (e.Response.ReportID == 0x80)
            {
                PSVRReport.PSVRDeviceInfoReport dev = PSVRReport.PSVRDeviceInfoReport.ParseInfo(e.Response.Data);
                serial = dev.SerialNumber;

                if (DeviceStatusChanged != null)
                    DeviceStatusChanged(this, new StatusEventArgs { Connected = true, SerialNumber = serial });
                
            }
        }












        public async Task<bool> HeadsetOn()
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.HeadsetOn();
            }
            else
            {
                if (client == null)
                    return false;

                return await client.HeadsetOn();
            }
        }

        public async Task<bool> HeadsetOff()
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.HeadsetOff();
            }
            else
            {
                if (client == null)
                    return false;

                return await client.HeadsetOff();
            }
        }

        public async Task<bool> EnableVRTracking()
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.EnableVRTracking();
            }
            else
            {
                if (client == null)
                    return false;

                return await client.EnableVRTracking();
            }
        }

        public async Task<bool> EnableVRMode()
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.EnableVRMode();
            }
            else
            {
                if (client == null)
                    return false;

                return await client.EnableVRMode();
            }
        }

        public async Task<bool> EnableCinematicMode()
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.EnableCinematicMode();
            }
            else
            {
                if (client == null)
                    return false;

                return await client.EnableCinematicMode();
            }
        }

        public async Task<bool> Shutdown()
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.Shutdown();
            }
            else
            {
                if (client == null)
                    return false;

                return await client.Shutdown();
            }
        }

        public async Task<bool> LedsOn()
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.LedsOn();
            }
            else
            {
                if (client == null)
                    return false;

                return await client.LedsOn();
            }
        }

        public async Task<bool> LedsOff()
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.LedsOff();
            }
            else
            {
                if (client == null)
                    return false;

                return await client.LedsOff();
            }
        }

        public async Task<bool> LedsDefault()
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.LedsDefault();
            }
            else
            {
                if (client == null)
                    return false;

                return await client.LedsDefault();
            }
        }

        public async Task<bool> RequestDeviceState()
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.RequestDeviceInfo();
            }
            else
            {
                if (client == null)
                    return false;

                return await client.RequestDeviceState();
            }
        }

        public async Task<bool> ResetPose()
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.ResetPose();
            }
            else
            {
                if (client == null)
                    return false;

                return await client.ResetPose();
            }
        }

        public async Task<bool> RecalibrateDevice()
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.RecalibrateDevice();
            }
            else
            {
                if (client == null)
                    return false;

                return await client.RecalibrateDevice();
            }
        }

        public async Task<bool> ApplyCinematicSettings(byte ScreenDistance, byte ScreenSize, byte Brightness, byte MicFeedback)
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.ApplyCinematicSettings(ScreenDistance, ScreenSize, Brightness, MicFeedback);
            }
            else
            {
                if (client == null)
                    return false;

                return await client.ApplyCinematicSettings(ScreenDistance, ScreenSize, Brightness, MicFeedback);
            }
        }

        public async Task<bool> ApplyLedSettings(byte[] Values)
        {
            if (local)
            {
                if (device == null)
                    return false;

                return device.Controller.ApplyLedSettings(Values, 0);
            }
            else
            {
                if (client == null)
                    return false;

                return await client.ApplyLedSettings(Values);
            }
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
            if (device != null)
            {
                device.Dispose();
                device = null;
            }

            DeviceStatusChanged = null;
            InputUpdate = null;
        }
    }
}
