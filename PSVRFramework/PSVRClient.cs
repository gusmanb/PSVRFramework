using System;
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
     * Frame format:
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

    public class PSVRClient
    {
        TcpClient client;
        Guid id;
        NetworkStream stream;
        CancellationTokenSource source;
        internal CancellationTokenSource cancelUpdateSource;

        public event EventHandler Closed;
        public event EventHandler<ExecutionEventArgs> ExecutionResult;
        public event EventHandler<StatusEventArgs> StatusUpdate;
        public event EventHandler<InputEventArgs> InputUpdate;

        public Guid Id { get { return id; } }

        bool disposed = true;

        int pingCount = 0;

        public bool Disposed { get { return disposed; } }

        public PSVRClient(IPAddress Address, int Port)
        {
            id = Id;
            client = new TcpClient();
            client.Connect(new System.Net.IPEndPoint(Address, Port));
            stream = client.GetStream();
            source = new CancellationTokenSource();
            StartListen(source.Token);
        }

        private async void StartListen(CancellationToken CancelToken)
        {
            int pos = 0;
            byte[] buffer = new byte[4096];

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
                            SendPong();
                        else
                            ProcessPacket(data);

                    }
                }
            }
            catch { }
            finally
            {
                Dispose();
            }
        }

        private void ProcessPacket(byte[] data)
        {

            if (data == null || data.Length == 0)
                throw new Exception("Malformed data");

            switch (data[0])
            {
                
                case 1:

                    if (data.Length != 2)
                        throw new InvalidCastException();

                    if (ExecutionResult != null)
                        ExecutionResult(this, new ExecutionEventArgs { Executed = data[1] == 1 });

                    break;

                case 2:

                    if(data.Length != 0x12)
                        throw new InvalidCastException();
                    
                    byte state = data[1];

                    float w = BitConverter.ToSingle(data, 2);
                    float x = BitConverter.ToSingle(data, 6);
                    float y = BitConverter.ToSingle(data, 10);
                    float z = BitConverter.ToSingle(data, 14);

                    PSVRInputState sstate = new PSVRInputState
                    {
                        PlusButton = (state & 1) == 1,
                        Lessbutton = (state & 2) == 2,
                        MuteButton = (state & 4) == 4,
                        Worn = (state & 8) == 8,
                        DisplayActive = (state & 16) == 16,
                        Muted = (state & 32) == 32,
                        Earphones = (state & 64) == 64,
                        Pose = new Quaternion(x, y, z, w)
                    };

                    if (InputUpdate != null)
                        InputUpdate(this, new InputEventArgs { State = sstate });

                    break;

                case 3:

                    if(data.Length < 2)
                        throw new InvalidCastException();

                    bool on = data[1] == 1;
                    string serial = "";

                    if (data.Length > 2)
                        Encoding.ASCII.GetString(data, 2, data.Length - 2);

                    if (StatusUpdate != null)
                        StatusUpdate(this, new StatusEventArgs { Connected = on, SerialNumber = serial });

                    break;

                default:

                    throw new InvalidCastException();
            }
        }

        private void SendPong()
        {
            stream.WriteAsync(new byte[] { 2, 0 }, 0, 2);
        }

        public async Task Send(byte[] Packet)
        {
            if (!disposed)
                await stream.WriteAsync(Packet, 0, Packet.Length);
        }

        public async Task<bool> HeadsetOn()
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {
                await Send(ForgeSimpleCommand(PSVRCommandId.HeadsetOn));
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> HeadsetOff()
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {
                await Send(ForgeSimpleCommand(PSVRCommandId.HeadsetOff));
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> EnableVRTracking()
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {
                await Send(ForgeSimpleCommand(PSVRCommandId.EnableVRTracking));
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> EnableVRMode()
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {
                await Send(ForgeSimpleCommand(PSVRCommandId.EnableVRMode));
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> EnableCinematicMode()
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {
                await Send(ForgeSimpleCommand(PSVRCommandId.EnableCinematicMode));
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> Shutdown()
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {
                await Send(ForgeSimpleCommand(PSVRCommandId.Shutdown));
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> LedsOn()
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {
                await Send(ForgeSimpleCommand(PSVRCommandId.LedsOn));
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> LedsOff()
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {
                await Send(ForgeSimpleCommand(PSVRCommandId.LedsOff));
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> LedsDefault()
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {
                await Send(ForgeSimpleCommand(PSVRCommandId.LedsDefault));
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> RequestDeviceState()
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {
                await Send(ForgeSimpleCommand(PSVRCommandId.RequestDeviceState));
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> ResetPose()
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {
                await Send(ForgeSimpleCommand(PSVRCommandId.ResetPose));
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> RecalibrateDevice()
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {
                await Send(ForgeSimpleCommand(PSVRCommandId.RecalibrateDevice));
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> ApplyCinematicSettings(byte ScreenDistance, byte ScreenSize, byte Brightness, byte MicFeedback)
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {

                byte[] packet = new byte[7];

                packet[0] = 7;
                packet[1] = 1;
                packet[2] = (byte)PSVRCommandId.ApplyCinematicSettings;
                packet[3] = ScreenDistance;
                packet[4] = ScreenSize;
                packet[5] = Brightness;
                packet[6] = MicFeedback;

                await Send(packet);

                return true;
            }
            catch { return false; }
        }

        public async Task<bool> ApplyLedSettings(byte[] Values)
        {
            if (disposed || client == null || stream == null || Values == null || Values.Length != 9)
                return false;

            try
            {

                byte[] packet = new byte[12];

                packet[0] = 7;
                packet[1] = 1;
                packet[2] = (byte)PSVRCommandId.ApplyCinematicSettings;
                packet[3] = Values[0];
                packet[4] = Values[1];
                packet[5] = Values[2];
                packet[6] = Values[3];
                packet[7] = Values[4];
                packet[8] = Values[5];
                packet[9] = Values[6];
                packet[10] = Values[7];
                packet[11] = Values[8];

                await Send(packet);

                return true;
            }
            catch { return false; }
        }

        public async Task<bool> ChangeInputUpdates(byte Interval)
        {
            if (disposed || client == null || stream == null)
                return false;

            try
            {
                await Send(new byte[] { 3, 2, Interval });
                return true;
            }
            catch { return false; }

        }

        private static byte[] ForgeSimpleCommand(PSVRCommandId Command)
        {
            return new byte[] { 3, 1, (byte)Command };
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

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
            StatusUpdate = null;
            ExecutionResult = null;
            InputUpdate = null;
        }

        enum PSVRCommandId : byte
        {
            HeadsetOn,
            HeadsetOff,
            EnableVRTracking,
            EnableVRMode,
            EnableCinematicMode,
            LedsOn,
            LedsOff,
            LedsDefault,
            ResetPose,
            RecalibrateDevice,
            Shutdown,
            ApplyCinematicSettings,
            ApplyLedSettings,
            RequestDeviceState
        }
        
    }

    public class StatusEventArgs : EventArgs
    {
        public string SerialNumber { get; set; }
        public bool Connected { get; set; }
    }

    public class ExecutionEventArgs : EventArgs
    {
        public bool Executed { get; set; }
    }

    public class InputEventArgs : EventArgs
    {
        public PSVRInputState State { get; set; }
    }

    public class PSVRInputState
    {
        public volatile bool PlusButton;
        public volatile bool Lessbutton;
        public volatile bool MuteButton;
        public volatile bool Worn;
        public volatile bool DisplayActive;
        public volatile bool Muted;
        public volatile bool Earphones;
        public Quaternion Pose;
    }
}
