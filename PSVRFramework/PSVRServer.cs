﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSVRFramework
{

    /*
     * Server Frame format:
     * 
     * 0x00 = size of packet
     * 0x01 = packet type
     * ~ = data
     * 
     * 
     * Server packet types:
     * 
     * 0 = ping
     * 1 = execute result
     * 2 = Input update
     * 3 = Device state (can be requested by the client of fired by the server)
     * 
     * 
     * Execute result response:
     * 
     * 0x00: result (0 = false, 1 = true)
     * 
     * 
     * Input update format:
     * 
     * 0x00 = Status byte- > b0 = present, b1= plus button, b2 = less button, b3 = mute button, b4 = worn, b5 = display active, b6 = muted, b7 = earphones
     * 0x01 - 0x04 = Pose W component
     * 0x05 - 0x08 = Pose X component
     * 0x09 - 0x0C = Pose Y component
     * 0x0D - 0x10 = Pose Z component
     * 
     * 
     * Device state format:
     * 
     * 0x00 = Status, 1 = connected, 2 = disconnected
     * ~ = Device serial
     * 
     * Client packet types
     * 
     * 0 = pong
     * 1 = execute function
     * 2 = control updates
     * 
     * 
     * Execute function format
     * 
     * 0x00 = Function ID
     * ~ = Function data
     * 
     * 
     * Control updates format
     * 
     * 0x00 = update interval (0 = disable)
     * 
     * 
     * Available commands:
     * 
     * HeadsetOn              = 0x00
     * HeadsetOff             = 0x01
     * EnableVRTracking       = 0x02
     * EnableVRMode           = 0x03
     * EnableCinematicMode    = 0x04
     * LedsOn                 = 0x05
     * LedsOff                = 0x06
     * LedsDefault            = 0x07
     * ResetPose              = 0x08
     * RecalibrateDevice      = 0x09
     * Shutdown               = 0x0A
     * ApplyCinematicSettings = 0x0B
     * ApplyLedSettings       = 0x0C
     * RequestDeviceState     = 0x0D 
     * 
     * ApplyCinematicSettings parameters format
     * 
     * 0x00 = Screen distance
     * 0x01 = Screen size
     * 0x02 = Brightness
     * 0x03 = MicFeedback
     * 
     * 
     * ApplyLedSettings parameters format
     * 
     * 0x00 - 0x08 = LED intensity (each byte one led)
     * 
     */

    public class PSVRServer : IDisposable
    {
        TcpListener listener;
        ConcurrentDictionary<Guid, PSVRServerClient> connectedClients = new ConcurrentDictionary<Guid, PSVRServerClient>();
        PSVRDevice device;
        PSVRSensorReport lastReport;

        string serial = "";
        CancellationTokenSource cancel;

        bool broadcasted = false;

        public PSVRDevice Device { get { return device; } }

        public PSVRServer(IPAddress Address, int Port)
        {
            cancel = new CancellationTokenSource();
            DeviceDetect(cancel.Token);
            Listen(Address, Port, cancel.Token);
        }

        async void Listen(IPAddress Address, int Port, CancellationToken CancelToken)
        {
            listener = new TcpListener(Address, Port);
            listener.Start();

            try
            {

                while (!CancelToken.IsCancellationRequested)
                {
                    var client = await Task.Run(() => listener.AcceptTcpClientAsync(), CancelToken);

                    try
                    {
                        Guid id = Guid.NewGuid();
                        var cclient = connectedClients[id] = new PSVRServerClient(client, id);
                        cclient.Received += Cclient_Received;
                        cclient.Closed += Cclient_Closed;
                    }
                    catch { }
                }
            }
            catch { }

            listener.Stop();
            listener = null;
        }

        private void Cclient_Closed(object sender, EventArgs e)
        {
            PSVRServerClient dummy;
            connectedClients.TryRemove((sender as PSVRServerClient).Id, out dummy);
        }

        private void Cclient_Received(object sender, ReceivedEventArgs e)
        {

            var client = sender as PSVRServerClient;

            byte[] data = e.Data;

            switch (data[0])
            {
                case 1:

                    bool response = false;

                    switch (data[1])
                    {
                        case 0:
                            if (data.Length != 2)
                                client.Dispose();
                            else
                                response = device?.Controller.HeadsetOn() ?? false;
                            break;
                        case 1:
                            if (data.Length != 2)
                                client.Dispose();
                            else
                                response = device?.Controller.HeadsetOff() ?? false;
                            break;
                        case 2:
                            if (data.Length != 2)
                                client.Dispose();
                            else
                                response = device?.Controller.EnableVRTracking() ?? false;
                            break;
                        case 3:
                            if (data.Length != 2)
                                client.Dispose();
                            else
                                response = device?.Controller.EnableVRMode() ?? false;
                            break;
                        case 4:
                            if (data.Length != 2)
                                client.Dispose();
                            else
                                response = device?.Controller.EnableCinematicMode() ?? false;
                            break;
                        case 5:
                            if (data.Length != 2)
                                client.Dispose();
                            else
                                response = device?.Controller.LedsOn() ?? false;
                            break;
                        case 6:
                            if (data.Length != 2)
                                client.Dispose();
                            else
                                response = device?.Controller.LedsOff() ?? false;
                            break;
                        case 7:
                            if (data.Length != 2)
                                client.Dispose();
                            else
                                response = device?.Controller.LedsDefault() ?? false;
                            break;
                        case 8:
                            if (data.Length != 2)
                                client.Dispose();
                            else
                                response = device?.Controller.ResetPose() ?? false;
                            break;
                        case 9:
                            if (data.Length != 2)
                                client.Dispose();
                            else
                                response = device?.Controller.Recalibrate() ?? false;
                            break;
                        case 10:
                            if (data.Length != 2)
                                client.Dispose();
                            else
                                response = device?.Controller.Shutdown() ?? false;
                            break;
                        case 11:
                            if (data.Length != 2)
                                client.Dispose();
                            else
                            {
                                response = true;
                                client.Send(PacketForger.ForgeDeviceStatus(device == null, device == null ? "" : serial));
                            }
                            break;
                        case 12:
                            if (data.Length != 5)
                                client.Dispose();
                            else
                                response = device?.Controller.ApplyCinematicSettings(data[1], data[2], data[3], data[4]) ?? false;
                            break;
                        case 13:
                            if (data.Length != 10)
                                client.Dispose();
                            else
                                response = device?.Controller.ApplyLedSettings(data, 1) ?? false;
                            break;
                        default:
                            client.Dispose();
                            break;

                    }

                    if(!client.Disposed)
                        client.Send(PacketForger.ForgeResponse(response));

                    break;

                case 2:
                    
                    if (data.Length != 2)
                        client.Dispose();
                    else
                    {
                        if (client.cancelUpdateSource != null)
                            client.cancelUpdateSource.Cancel();

                        client.cancelUpdateSource = new CancellationTokenSource();
                        PSVRServer server = this;
                        byte interval = data[1];

                        if (interval != 0)
                        {
                            Task.Run(async () =>
                            {
                                await Task.Delay(interval, cancel.Token);
                                var ddata = server.lastReport;
                                if (ddata != null)
                                {
                                    data = PacketForger.ForgeInputState(ddata);
                                    client.Send(data);
                                }
                            });
                        }

                        client.Send(PacketForger.ForgeResponse(true));
                    }

                    break;

                default:
                    client.Dispose();
                    break;
            }
        }

        async void DeviceDetect(CancellationToken CancelToken)
        {
            while (!CancelToken.IsCancellationRequested)
            {

                if (device == null)
                {
                    device = PSVRDevice.GetDevice();

                    if (device != null)
                    {

                        //Headset on? shure? maybe 0x10 is used to power up the box without powering on the headset

                        device.Removed += Device_Removed;
                        device.INReport += Device_INReport;
                        device.Controller.RequestDeviceInfo();
                        device.Controller.HeadsetOn();
                        device.Controller.RequestDeviceInfo();
                    }

                    await Task.Delay(100, CancelToken);
                }
                else
                    await Task.Delay(1000, CancelToken);
            }
        }
        
        private void Device_INReport(object sender, PSVRINEventArgs e)
        {
            if (e.Response.ReportID == 0x80)
            {
                PSVRReport.PSVRDeviceInfoReport dev = PSVRReport.PSVRDeviceInfoReport.ParseInfo(e.Response.Data);
                serial = dev.SerialNumber;

                if (!broadcasted)
                {
                    broadcasted = true;
                    BroadcastDeviceConnected();
                }

            }
        }

        void BroadcastDeviceConnected()
        {
            byte[] packet = PacketForger.ForgeDeviceStatus(true, serial);

            foreach (var client in connectedClients.Values)
                client.Send(packet);
        }

        private void Device_Removed(object sender, EventArgs e)
        {
            device = null;
            byte[] packet = PacketForger.ForgeDeviceStatus(false, serial);

            foreach (var client in connectedClients.Values)
            {
                client.Send(packet);
            }

            lastReport = null;
            device = null;
        }

        public void Dispose()
        {
            foreach (var client in connectedClients.Values)
                client.Dispose();

            connectedClients.Clear();
            cancel.Cancel();

            if (device != null)
                device.Dispose();
        }

        static class PacketForger
        {
            internal static byte[] ForgeDeviceStatus(bool connected, string serial)
            {
                byte[] nameData = Encoding.ASCII.GetBytes(serial);
                byte[] data = new byte[nameData.Length + 3];
                data[0] = (byte)nameData.Length;
                data[1] = 3;
                data[2] = (byte)(connected ? 1 : 0);
                Buffer.BlockCopy(nameData, 0, data, 3, nameData.Length);

                return data;
            }

            internal static byte[] ForgeInputState(PSVRSensorReport Report)
            {
                byte[] data = new byte[19];
                data[0] = 19;
                data[1] = 2;

                byte but = (byte)(Report.Buttons.HasFlag(PSVRSensorReport.HeadsetButtons.VolUp) ? (1 << 0) : 0);
                but = (byte)(but | (byte)(Report.Buttons.HasFlag(PSVRSensorReport.HeadsetButtons.VolDown) ? (1 << 1) : 0));
                but = (byte)(but | (byte)(Report.Buttons.HasFlag(PSVRSensorReport.HeadsetButtons.Mute) ? (1 << 2) : 0));
                but = (byte)(but | (byte)(Report.Worn ? (1 << 3) : 0));
                but = (byte)(but | (byte)(Report.DisplayActive ? (1 << 4) : 0));
                but = (byte)(but | (byte)(Report.Muted ? (1 << 5) : 0));
                but = (byte)(but | (byte)(Report.EarphonesConnected ? (1 << 6) : 0));

                data[2] = but;

                byte[] tmp = BitConverter.GetBytes(Report.Pose.W);
                Buffer.BlockCopy(tmp, 0, data, 3, 4);

                tmp = BitConverter.GetBytes(Report.Pose.W);
                Buffer.BlockCopy(tmp, 0, data, 7, 4);

                tmp = BitConverter.GetBytes(Report.Pose.W);
                Buffer.BlockCopy(tmp, 0, data, 11, 4);

                tmp = BitConverter.GetBytes(Report.Pose.W);
                Buffer.BlockCopy(tmp, 0, data, 15, 4);

                return data;
            }

            internal static byte[] ForgeResponse(bool Success)
            {
                return new byte[] { 3, 1, (byte)(Success ? 1 : 0) };
            }
        }

    }

    public class PSVRServerClient : IDisposable
    {
        TcpClient client;
        Guid id;
        NetworkStream stream;
        CancellationTokenSource source;
        internal CancellationTokenSource cancelUpdateSource;

        public event EventHandler Closed;
        public event EventHandler<ReceivedEventArgs> Received;
        public Guid Id { get { return id; } }

        Timer pingTimer;
        bool disposed = true;

        int pingCount = 0;

        public bool Disposed { get { return disposed; } }

        public PSVRServerClient(TcpClient Client, Guid Id)
        {
            id = Id;
            client = Client;
            stream = Client.GetStream();
            source = new CancellationTokenSource();
            StartListen(source.Token);
            pingTimer = new Timer(PingElapsed, null, 10000, 10000);
        }

        private async void StartListen(CancellationToken CancelToken)
        {
            int pos = 0;
            byte[] buffer = new byte[1024];

            try
            {

                while (!CancelToken.IsCancellationRequested)
                {
                    var read = await stream.ReadAsync(buffer, pos, buffer.Length - pos);

                    if (read == 0)
                        throw new InvalidOperationException();

                    while (buffer[0] <= pos)
                    {
                        int packetSize = buffer[0]; 
                        byte[] data = new byte[packetSize - 1];
                        Buffer.BlockCopy(buffer, 1, data, 0, data.Length);
                        pos = pos - packetSize;
                        Buffer.BlockCopy(buffer, packetSize, buffer, 0, pos);

                        if (data[0] == 2 && data[1] == 0)
                            ResetPing();
                        else if (Received != null)
                            Received(this, new ReceivedEventArgs { Data = data });
                            
                    }
                }
            }
            catch { }
            finally
            {
                Dispose();
            }
        }

        void PingElapsed(object state)
        {
            if (pingCount++ > 3)
                Dispose();

            Send(new byte[] { 2, 0 });
        }

        private void ResetPing()
        {
            pingCount = 0;
        }

        public async void Send(byte[] Packet)
        {
            if (!disposed)
                await stream.WriteAsync(Packet, 0, Packet.Length);
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            pingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            pingTimer = null;

            if (Closed != null)
                Closed(this, EventArgs.Empty);

            source.Cancel();
            source.Dispose();
            stream.Close();
            stream.Dispose();
            client.Close();

            source = null;
            stream = null;
            client = null;
            Closed = null;
            Received = null;
            
        }
    }

    public class ReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }


    //public struct PSVRStatus
    //{
    //    public volatile bool Present;
    //    public volatile bool PlusButton;
    //    public volatile bool Lessbutton;
    //    public volatile bool MuteButton;
    //    public volatile bool Worn;
    //    public volatile bool DisplayActive;
    //    public volatile bool Muted;
    //    public volatile bool Earphones;
    //    public Quaternion Pose;
    //}
}