using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibUsbDotNet;
using LibUsbDotNet.LudnMonoLibUsb;
using LibUsbDotNet.WinUsb;
using MonoLibUsb;
using System.Runtime.InteropServices;

namespace PSVRFramework
{
    public class PSVRSensor
    {
        //To be analyzed...
        public HeadsetButtons Buttons;
        public int Volume;
        public bool Worn;
        public bool DisplayActive;
        public bool Muted;
        public bool EarphonesConnected;

        public int GroupSequence1;

        public int GyroYaw1;
        public int GyroPitch1;
        public int GyroRoll1;

        public int MotionX1;
        public int MotionY1;
        public int MotionZ1;

        public int GroupSequence2;

        public int GyroYaw2;
        public int GyroPitch2;
        public int GyroRoll2;

        public int MotionX2;
        public int MotionY2;
        public int MotionZ2;


        public int IRSensor; //1023 = near, 0 = far

        public int CalStatus; //Calibration status? 255 = boot, 0-3 calibrating? 4 = calibrated, maybe a bit mask with sensor status? (0 bad, 1 good)?
        public int Ready; //0 = not ready, 1 = ready -> uint ushort or byte?

        public int PacketSequence;

        public int VoltageReference; //Not sure at all, starts in 0 and suddenly jumps to 3
        public int VoltageValue; //Not sure at all, starts on 0, ranges very fast to 255, when switched from VR to Cinematic and back varies between 255 and 254

        //DEBUG
        public int A;
        public int B;
        public int C;
        public int D;
        public int E;
        public int F;
        public int G;
        public int H;
        
    }

    

    [Flags]
    public enum HeadsetButtons
    {
        VolUp = 2,
        VolDown = 4,
        Mute = 8
    }

    public struct PSVRState
    {
        public PSVRSensor sensor;
    }
    
    public class PSVR : IDisposable
    {

#if DEBUG
        static PSVR()
        {
            UsbDevice.UsbErrorEvent += UsbDevice_UsbErrorEvent;
        }

        private static void UsbDevice_UsbErrorEvent(object sender, UsbError e)
        {
            Console.WriteLine("Error USB: " + e.Win32ErrorNumber + " - " + e.Description);
        }
#endif

        UsbEndpointWriter writer;
        UsbEndpointReader reader;
        UsbEndpointReader cmdReader;
        UsbDevice controlDevice;
        UsbDevice sensorDevice;

        public event EventHandler<PSVRSensorEventArgs> SensorDataUpdate;
        public event EventHandler Removed;
        public event EventHandler<PSVRResponseEventArgs> CommandResponse;

        Timer aliveTimer;
        
        public PSVR(bool EnableSensor)
        {

            if (CurrentOS.IsWindows)
            {
                var ndev = UsbDevice.AllDevices.Where(d => d.Vid == 0x54C && d.Pid == 0x09AF && d.SymbolicName.ToLower().Contains("mi_05")).FirstOrDefault();

                if (ndev == null)
                    throw new InvalidOperationException("No Control device found");

                if (!ndev.Open(out controlDevice))
                    throw new InvalidOperationException("Device in use");

                ndev = UsbDevice.AllDevices.Where(d => d.Vid == 0x54C && d.Pid == 0x09AF && d.SymbolicName.ToLower().Contains("mi_04")).FirstOrDefault();

                if (ndev == null)
                {
                    controlDevice.Close();
                    throw new InvalidOperationException("No Sensor device found");
                }

                if (EnableSensor)
                {
                    if (!ndev.Open(out sensorDevice))
                    {
                        controlDevice.Close();
                        throw new InvalidOperationException("Device in use");
                    }
                }

                writer = controlDevice.OpenEndpointWriter(LibUsbDotNet.Main.WriteEndpointID.Ep04);
                cmdReader = controlDevice.OpenEndpointReader(LibUsbDotNet.Main.ReadEndpointID.Ep04);
                cmdReader.DataReceived += CmdReader_DataReceived;
                cmdReader.DataReceivedEnabled = true;

                if (EnableSensor)
                {
                    reader = sensorDevice.OpenEndpointReader(LibUsbDotNet.Main.ReadEndpointID.Ep03, 64);
                    reader.DataReceived += Reader_DataReceived;
                    reader.DataReceivedEnabled = true;
                }

                aliveTimer = new Timer(is_alive);
                aliveTimer.Change(2000, 2000);
            }
            else
            {
                var found = UsbDevice.AllDevices.Where(d => d.Vid == 0x54C && d.Pid == 0x09AF).FirstOrDefault();

                controlDevice = found.Device;
                
                var dev = (MonoUsbDevice)controlDevice;

                var handle = new MonoUsbDeviceHandle(dev.Profile.ProfileHandle);

                MonoUsbApi.DetachKernelDriver(handle, 5);

                if(EnableSensor)
                    MonoUsbApi.DetachKernelDriver(handle, 4);

                if (!dev.ClaimInterface(5))
                {
                    controlDevice.Close();
                    throw new InvalidOperationException("Device in use");
                }

                if (EnableSensor)
                {
                    if (!dev.ClaimInterface(4))
                    {
                        controlDevice.Close();
                        throw new InvalidOperationException("Device in use");
                    }
                }

                writer = dev.OpenEndpointWriter(LibUsbDotNet.Main.WriteEndpointID.Ep04);

                if (EnableSensor)
                {
                    reader = dev.OpenEndpointReader(LibUsbDotNet.Main.ReadEndpointID.Ep03, 64);
                    reader.DataReceived += Reader_DataReceived;
                    reader.DataReceivedEnabled = true;
                }              
            }
            
        }

        private void CmdReader_DataReceived(object sender, LibUsbDotNet.Main.EndpointDataEventArgs e)
        {
            int pos = 0;

            while (pos < e.Count)
            {
                int consumed;

                PSVRMessage msg = PSVRMessage.ParseResponse(e.Buffer, pos, out consumed);
                if (CommandResponse != null)
                    CommandResponse(this, new PSVRResponseEventArgs { Response = msg });

                pos += consumed;
            }
        }

        void is_alive(object state)
        {
            try
            {
                if (!controlDevice.UsbRegistryInfo.IsAlive)
                {
                    aliveTimer.Change(Timeout.Infinite, Timeout.Infinite);

                    if (Removed != null)
                        Removed(this, EventArgs.Empty);

                    Dispose();
                }
            }
            catch { }
        }


        int up = 0;

        private void Reader_DataReceived(object sender, LibUsbDotNet.Main.EndpointDataEventArgs e)
        {

            var rep = parse(e.Buffer);

            if (SensorDataUpdate == null)
                return;
            
            SensorDataUpdate(this, new PSVRSensorEventArgs { SensorData = rep.sensor });
        }

        public bool SendCommand(PSVRMessage Command)
        {
            var data = Command.Serialize();
            int len;

            return writer.Write(data, 1000, out len) == LibUsbDotNet.Main.ErrorCode.None;
            
        }

        public static PSVRState parse(byte[] data)
        {
            PSVRState state = new PSVRState();
            state.sensor = parseSensor(data);
            return state;
        }
        
        public static PSVRSensor parseSensor(byte[] data)
        {

            PSVRSensor sensor = new PSVRSensor();
            if (data == null)
            {
                return sensor;
            }

            sensor.Buttons = (HeadsetButtons)data[0];
            sensor.Volume = data[2];

            sensor.Worn = (data[8] & 0x1) == 0x1 ? true : false;//confirmed
            sensor.DisplayActive = (data[8] & 0x2) == 0x2 ? false : true;
            sensor.Muted = (data[8] & 0x8) == 0x8 ? true : false;//confirmed

            sensor.EarphonesConnected = (data[8] & 0x10) == 0x10 ? true : false;//confirmed

            sensor.GroupSequence1 = getIntFromInt16(data, 18);

            sensor.GyroYaw1 = getIntFromInt16(data, 20);
            sensor.GyroPitch1 = getIntFromInt16(data, 22);
            sensor.GyroRoll1 = getIntFromInt16(data, 24);

            sensor.MotionX1 = getIntFromInt16(data, 26) ;
            sensor.MotionY1 = getIntFromInt16(data, 28);
            sensor.MotionZ1 = getIntFromInt16(data, 30);
            
            sensor.GroupSequence2 = getIntFromInt16(data, 34);

            sensor.GyroYaw2 = getIntFromInt16(data, 36);
            sensor.GyroPitch2 = getIntFromInt16(data, 38);
            sensor.GyroRoll2 = getIntFromInt16(data, 40);

            sensor.MotionX2 = getIntFromInt16(data, 42);
            sensor.MotionY2 = getIntFromInt16(data, 44);
            sensor.MotionZ2 = getIntFromInt16(data, 46);
            
            sensor.CalStatus = data[48];
            sensor.Ready = data[49];

            sensor.A = data[50];
            sensor.B = data[51];
            sensor.C = data[52];

            sensor.VoltageValue = data[53];
            sensor.VoltageReference = data[54];
            sensor.IRSensor = getIntFromInt16(data, 55);

            sensor.D = data[58];
            sensor.E = data[59];
            sensor.F = data[60];
            sensor.G = data[61];
            sensor.H = data[62];

            sensor.PacketSequence = data[63];
            
            return sensor;

        }

        private static int convert(byte byte1, byte byte2)
        {
            return (short)byte1 | (short)(byte2 << 8);
        }

        private static int getIntFromInt16(byte[] data, byte offset)
        {

            return (short)data[offset] | (short)(data[offset + 1] << 8);
        }

        private static short getAccelShort(byte[] data, byte offset)
        {

            return (short)(((short)data[offset] | (short)(data[offset + 1] << 8)) >> 4);
        }

        private static int getIntFromUInt16(byte[] data, byte offset)
        {

            return (ushort)data[offset] | (ushort)(data[offset + 1] << 8);
        }

        public void Dispose()
        {
            try
            {
                if(aliveTimer != null)
                    aliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            catch { }

            try
            {
                reader.Dispose();
                writer.Dispose();
            }
            catch { }

            try
            {
                controlDevice.Close();
            }
            catch { }

            try
            {
                sensorDevice.Close();
            }
            catch { }
            
            controlDevice = null;

            SensorDataUpdate = null;
            Removed = null;
            CommandResponse = null;
        }
        
    }

    public class PSVRDeviceInfo
    {
        public ushort UnknownA;
        public byte MinorVersion;
        public byte MajorVersion;
        public ushort UnknownB;
        public uint UnknownC;
        public ushort UnknownD;
        public string SerialNumber; //16 bytes
        public uint UnknownE;
        public uint UnknownF;
        public uint UnknownG;
        public uint UnknownH;
        public uint UnknownI;

        public static PSVRDeviceInfo ParseInfo(byte[] Data)
        {
            PSVRDeviceInfo info = new PSVRDeviceInfo();

            info.UnknownA = (ushort)(Data[0] | ((ushort)(Data[1] << 8)));
            info.MinorVersion = Data[2];
            info.MajorVersion = Data[3];
            info.UnknownB = (ushort)(Data[4] | ((ushort)(Data[5] << 8)));
            info.UnknownC = (uint)(Data[6] | ((uint)(Data[7] << 8)) | ((uint)(Data[8] << 16)) | ((uint)(Data[9] << 24)));
            info.UnknownD = (ushort)(Data[10] | ((ushort)(Data[11] << 8)));
            info.SerialNumber = Encoding.ASCII.GetString(Data, 12, 16);
            info.UnknownE = (uint)(Data[28] | ((uint)(Data[29] << 8)) | ((uint)(Data[30] << 16)) | ((uint)(Data[31] << 24)));
            info.UnknownF = (uint)(Data[32] | ((uint)(Data[33] << 8)) | ((uint)(Data[34] << 16)) | ((uint)(Data[35] << 24)));
            info.UnknownG = (uint)(Data[36] | ((uint)(Data[37] << 8)) | ((uint)(Data[38] << 16)) | ((uint)(Data[39] << 24)));
            info.UnknownH = (uint)(Data[40] | ((uint)(Data[41] << 8)) | ((uint)(Data[42] << 16)) | ((uint)(Data[43] << 24)));
            info.UnknownH = (uint)(Data[44] | ((uint)(Data[45] << 8)) | ((uint)(Data[46] << 16)) | ((uint)(Data[47] << 24)));
            
            return info;
        }
    }

    public class PSVRDeviceStatus
    {
        public PSVRStatusMask Status;
        public uint Volume;
        public ushort UnknownA;
        public byte BridgeOutputID;
        public byte UnknownB;

        public static PSVRDeviceStatus ParseStatus(byte[] Data)
        {
            PSVRDeviceStatus info = new PSVRDeviceStatus();

            info.Status = (PSVRStatusMask)Data[0];
            info.Volume = (uint)(Data[1] | ((uint)(Data[2] << 8)) | ((uint)(Data[3] << 16)) | ((uint)(Data[4] << 24)));
            info.UnknownA = (ushort)(Data[5] | ((ushort)(Data[6] << 8)));
            info.BridgeOutputID = Data[7];
            info.UnknownB = Data[7];

            return info;
        }

        [Flags]
        public enum PSVRStatusMask
        {
            HeadsetOn =  (1 << 0),
            Worn =       (1 << 1),
            Cinematic =  (1 << 2),
            UnknownA =   (1 << 3),
            Headphones = (1 << 4),
            Mute =       (1 << 5),
            UnknownB =   (1 << 6),
            UnknownC =   (1 << 7),
        }
    }

    public class PSVRSensorEventArgs : EventArgs
    {
        public PSVRSensor SensorData { get; set; }
    }

    public class PSVRResponseEventArgs : EventArgs
    {
        public PSVRMessage Response { get; set; }
    }

    public struct PSVRMessage
    {
        public byte r_id;
        public byte command_status;
        public byte magic;
        public byte length;
        public byte[] data;
        public byte[] Serialize()
        {
            byte[] data = new byte[64];
            data[0] = r_id;
            data[1] = command_status;
            data[2] = magic;
            data[3] = length;

            Buffer.BlockCopy(this.data, 0, data, 4, length);

            return data;
        }

        public static PSVRMessage ParseResponse(byte[] Data, int Offset, out int BytesConsumed)
        {
            PSVRMessage cmd = new PSVRMessage();
            cmd.r_id = Data[Offset];
            cmd.command_status = Data[Offset + 1];
            cmd.magic = Data[Offset + 2];
            cmd.length = Data[Offset + 3];
            cmd.data = new byte[cmd.length];
            Buffer.BlockCopy(Data, Offset + 4, cmd.data, 0, cmd.length);
            BytesConsumed = cmd.length + 4;
            return cmd;
        }

        public static PSVRMessage GetEnableVRTracking()
        {
            PSVRMessage cmd = new PSVRMessage();

            cmd.r_id = 0x11;
            cmd.magic = 0xaa;
            cmd.length = 8;
            byte[] data = new byte[8];
            byte[] tmp = BitConverter.GetBytes(0xFFFFFF00);
            Buffer.BlockCopy(tmp, 0, data, 0, 4);
            tmp = BitConverter.GetBytes(0x00000000);
            Buffer.BlockCopy(tmp, 0, data, 4, 4);
            cmd.data = data;

            return cmd;
        }

        public static PSVRMessage GetHeadsetOn()
        {
            PSVRMessage cmd = new PSVRMessage();

            cmd.r_id = 0x17;
            cmd.magic = 0xaa;
            cmd.length = 4;
            cmd.data = BitConverter.GetBytes(0x00000001);

            return cmd;
        }

        public static PSVRMessage GetHeadsetOff()
        {
            PSVRMessage cmd = new PSVRMessage();

            cmd.r_id = 0x17;
            cmd.magic = 0xaa;
            cmd.length = 4;
            cmd.data = BitConverter.GetBytes(0x00000000);

            return cmd;
        }

        public static PSVRMessage GetEnterVRMode()
        {
            PSVRMessage cmd = new PSVRMessage();

            cmd.r_id = 0x23;
            cmd.magic = 0xaa;
            cmd.length = 4;
            cmd.data = BitConverter.GetBytes(0x00000001);

            return cmd;
        }

        public static PSVRMessage GetExitVRMode()
        {
            PSVRMessage cmd = new PSVRMessage();

            cmd.r_id = 0x23;
            cmd.magic = 0xaa;
            cmd.length = 4;
            cmd.data = BitConverter.GetBytes(0x00000000);

            return cmd;
        }

        public static PSVRMessage GetOff(byte id)
        {
            PSVRMessage cmd = new PSVRMessage();

            cmd.r_id = id;
            cmd.magic = 0xaa;
            cmd.length = 4;
            cmd.data = BitConverter.GetBytes(0x00000000);

            return cmd;
        }

        public static PSVRMessage GetOn(byte id)
        {
            PSVRMessage cmd = new PSVRMessage();

            cmd.r_id = id;
            cmd.magic = 0xaa;
            cmd.length = 4;
            cmd.data = BitConverter.GetBytes(0x00000001);

            return cmd;
        }

        public static PSVRMessage GetBoxOff()
        {
            PSVRMessage cmd = new PSVRMessage();

            cmd.r_id = 0x13;
            cmd.magic = 0xaa;
            cmd.length = 4;
            cmd.data = BitConverter.GetBytes(0x00000001);

            return cmd;
        }
        
        public static PSVRMessage GetSetCinematicConfiguration(byte Mask, byte ScreenDistance, byte ScreenSize, byte IPD, byte Brightness, byte MicVolume, bool UnknownVRSetting)
        {
            PSVRMessage cmd = new PSVRMessage();

            cmd.r_id = 0x21;
            cmd.magic = 0xaa;
            cmd.length = 16;
            cmd.data = new byte[] { Mask, ScreenSize, ScreenDistance, IPD, 0, 0, 0, 0, 0, 0, Brightness, MicVolume, 0, 0, (byte)(UnknownVRSetting ? 0 : 1), 0 };

            return cmd;
        }

        public static PSVRMessage GetSetHDMLed(LedMask Mask, byte Value)
        {
            ushort mMask = (ushort)Mask;
            PSVRMessage cmd = new PSVRMessage();

            cmd.r_id = 0x15;
            cmd.magic = 0xaa;
            cmd.length = 16;
            cmd.data = new byte[] { (byte)(mMask & 0xFF), (byte)((mMask >> 8) & 0xFF), Value, Value, Value, Value, Value, Value, Value, Value, Value, 0, 0, 0, 0, 0 };
            return cmd;
        }

        public static PSVRMessage GetSetHDMLeds(LedMask Mask, byte ValueA, byte ValueB, byte ValueC, byte ValueD, byte ValueE, byte ValueF, byte ValueG, byte ValueH, byte ValueI)
        {
            ushort mMask = (ushort)Mask;
            PSVRMessage cmd = new PSVRMessage();

            cmd.r_id = 0x15;
            cmd.magic = 0xaa;
            cmd.length = 16;
            cmd.data = new byte[] { (byte)(mMask & 0xFF), (byte)((mMask >> 8) & 0xFF), ValueA, ValueB, ValueC, ValueD, ValueE, ValueF, ValueG, ValueH, ValueI, 0, 0, 0, 0, 0 };
            return cmd;
        }

        public static PSVRMessage GetReadDeviceInfo()
        {
            PSVRMessage cmd = new PSVRMessage();

            cmd.r_id = 0x81;
            cmd.magic = 0xaa;
            cmd.length = 8;
            cmd.data = new byte[] { 0x80, 0, 0, 0, 0, 0, 0, 0 };

            return cmd;
        }
    };

    [Flags]
    public enum LedMask : ushort
    {
        None = 0,
        LedA = (1 << 0),
        LedB = (1 << 1),
        LedC = (1 << 2),
        LedD = (1 << 3),
        LedE = (1 << 4),
        LedF = (1 << 5),
        LedG = (1 << 6),
        LedH = (1 << 7),
        LedI = (1 << 8),
        All = LedA | LedB | LedC | LedD | LedE | LedF | LedG | LedH | LedI
    }
}
