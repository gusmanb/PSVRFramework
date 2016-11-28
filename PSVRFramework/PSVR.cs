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
    public class PSVRSensorReport
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

        //
        public BMI055SensorData FilteredSensorReading1;
        public BMI055SensorData FilteredSensorReading2;

        static PSVRSensorReport()
        {
            BMI055Parser.Init(BMI055Parser.AScale.AFS_2G, BMI055Parser.Gscale.GFS_125DPS);
        }

        public static PSVRSensorReport ParseSensor(byte[] data, bool FilterSensors)
        {

            PSVRSensorReport sensor = new PSVRSensorReport();

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

            sensor.MotionX1 = getIntFromInt16(data, 26);
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

            if (FilterSensors)
            {
                sensor.FilteredSensorReading1 = BMI055Parser.Parse(data, 26, 20);
                sensor.FilteredSensorReading2 = BMI055Parser.Parse(data, 42, 36);
            }

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

        private static int getIntFromUInt16(byte[] data, byte offset)
        {

            return (ushort)data[offset] | (ushort)(data[offset + 1] << 8);
        }

        [Flags]
        public enum HeadsetButtons
        {
            VolUp = 2,
            VolDown = 4,
            Mute = 8
        }

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
        public event EventHandler<PSVRINEventArgs> INReport;

        Timer aliveTimer;

        bool filterSensor;

        public PSVR(bool EnableSensor, bool FilterSensor)
        {
            filterSensor = FilterSensor;

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

                PSVRReport msg = PSVRReport.ParseResponse(e.Buffer, pos, out consumed);
                if (INReport != null)
                    INReport(this, new PSVRINEventArgs { Response = msg });

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
        
        private void Reader_DataReceived(object sender, LibUsbDotNet.Main.EndpointDataEventArgs e)
        {
            var rep = PSVRSensorReport.ParseSensor(e.Buffer, filterSensor);

            if (SensorDataUpdate == null)
                return;
            
            SensorDataUpdate(this, new PSVRSensorEventArgs { SensorData = rep });
        }

        public bool SendReport(PSVRReport Report)
        {
            var data = Report.Serialize();
            int len;

            return writer.Write(data, 1000, out len) == LibUsbDotNet.Main.ErrorCode.None;
            
        }

        public bool SendRaw(byte[] Data)
        {
            int len;
            return writer.Write(Data, 1000, out len) == LibUsbDotNet.Main.ErrorCode.None;
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
            INReport = null;
        }
        
    }
    
    public struct PSVRReport
    {
        public byte ReportID;
        public byte CommandStatus;
        public byte DataStart;
        public byte DataLength;
        public byte[] Data;

        public byte[] Serialize()
        {
            byte[] data = new byte[64];
            data[0] = ReportID;
            data[1] = CommandStatus;
            data[2] = DataStart;
            data[3] = DataLength;

            if(this.Data != null)
                Buffer.BlockCopy(this.Data, 0, data, 4, DataLength);

            return data;
        }

        public static PSVRReport ParseResponse(byte[] Data, int Offset, out int BytesConsumed)
        {
            PSVRReport cmd = new PSVRReport();
            cmd.ReportID = Data[Offset];
            cmd.CommandStatus = Data[Offset + 1];
            cmd.DataStart = Data[Offset + 2];
            cmd.DataLength = Data[Offset + 3];
            cmd.Data = new byte[cmd.DataLength];
            Buffer.BlockCopy(Data, Offset + 4, cmd.Data, 0, cmd.DataLength);
            BytesConsumed = cmd.DataLength + 4;
            return cmd;
        }

        public static PSVRReport GetEnableVRTracking()
        {
            PSVRReport cmd = new PSVRReport();

            cmd.ReportID = 0x11;
            cmd.DataStart = 0xaa;
            cmd.DataLength = 8;
            byte[] data = new byte[8];
            byte[] tmp = BitConverter.GetBytes(0xFFFFFF00);
            Buffer.BlockCopy(tmp, 0, data, 0, 4);
            tmp = BitConverter.GetBytes(0x00000000);
            Buffer.BlockCopy(tmp, 0, data, 4, 4);
            cmd.Data = data;

            return cmd;
        }

        public static PSVRReport GetEnableVRTrackingTest(byte value)
        {
            PSVRReport cmd = new PSVRReport();

            cmd.ReportID = 0x11;
            cmd.DataStart = 0xaa;
            cmd.DataLength = 8;
            byte[] data = new byte[8];
            byte[] tmp = new byte[] { value, 0x00, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF };
            Buffer.BlockCopy(tmp, 0, data, 0, 8);
            tmp = BitConverter.GetBytes(0x00000000);
            Buffer.BlockCopy(tmp, 0, data, 4, 4);
            cmd.Data = data;

            return cmd;
        }

        public static PSVRReport GetHeadsetOn()
        {
            PSVRReport cmd = new PSVRReport();

            cmd.ReportID = 0x17;
            cmd.DataStart = 0xaa;
            cmd.DataLength = 4;
            cmd.Data = BitConverter.GetBytes(0x00000001);

            return cmd;
        }

        public static PSVRReport GetHeadsetOff()
        {
            PSVRReport cmd = new PSVRReport();

            cmd.ReportID = 0x17;
            cmd.DataStart = 0xaa;
            cmd.DataLength = 4;
            cmd.Data = BitConverter.GetBytes(0x00000000);

            return cmd;
        }

        public static PSVRReport GetEnterVRMode()
        {
            PSVRReport cmd = new PSVRReport();

            cmd.ReportID = 0x23;
            cmd.DataStart = 0xaa;
            cmd.DataLength = 4;
            cmd.Data = BitConverter.GetBytes(0x00000001);

            return cmd;
        }

        public static PSVRReport GetExitVRMode()
        {
            PSVRReport cmd = new PSVRReport();

            cmd.ReportID = 0x23;
            cmd.DataStart = 0xaa;
            cmd.DataLength = 4;
            cmd.Data = BitConverter.GetBytes(0x00000000);

            return cmd;
        }

        public static PSVRReport GetOff(byte id)
        {
            PSVRReport cmd = new PSVRReport();

            cmd.ReportID = id;
            cmd.DataStart = 0xaa;
            cmd.DataLength = 4;
            cmd.Data = BitConverter.GetBytes(0x00000000);

            return cmd;
        }

        public static PSVRReport GetOn(byte id)
        {
            PSVRReport cmd = new PSVRReport();

            cmd.ReportID = id;
            cmd.DataStart = 0xaa;
            cmd.DataLength = 4;
            cmd.Data = BitConverter.GetBytes(0x00000001);

            return cmd;
        }

        public static PSVRReport GetBoxOff()
        {
            PSVRReport cmd = new PSVRReport();

            cmd.ReportID = 0x13;
            cmd.DataStart = 0xaa;
            cmd.DataLength = 4;
            cmd.Data = BitConverter.GetBytes(0x00000001);

            return cmd;
        }
        
        public static PSVRReport GetSetCinematicConfiguration(byte Mask, byte ScreenDistance, byte ScreenSize, byte IPD, byte Brightness, byte MicVolume, bool UnknownVRSetting)
        {
            PSVRReport cmd = new PSVRReport();

            cmd.ReportID = 0x21;
            cmd.DataStart = 0xaa;
            cmd.DataLength = 16;
            cmd.Data = new byte[] { Mask, ScreenSize, ScreenDistance, IPD, 0, 0, 0, 0, 0, 0, Brightness, MicVolume, 0, 0, (byte)(UnknownVRSetting ? 0 : 1), 0 };

            return cmd;
        }

        public static PSVRReport GetSetHDMLed(LedMask Mask, byte Value)
        {
            ushort mMask = (ushort)Mask;
            PSVRReport cmd = new PSVRReport();

            cmd.ReportID = 0x15;
            cmd.DataStart = 0xaa;
            cmd.DataLength = 16;
            cmd.Data = new byte[] { (byte)(mMask & 0xFF), (byte)((mMask >> 8) & 0xFF), Value, Value, Value, Value, Value, Value, Value, Value, Value, 0, 0, 0, 0, 0 };
            return cmd;
        }

        public static PSVRReport GetSetHDMLeds(LedMask Mask, byte ValueA, byte ValueB, byte ValueC, byte ValueD, byte ValueE, byte ValueF, byte ValueG, byte ValueH, byte ValueI)
        {
            ushort mMask = (ushort)Mask;
            PSVRReport cmd = new PSVRReport();

            cmd.ReportID = 0x15;
            cmd.DataStart = 0xaa;
            cmd.DataLength = 16;
            cmd.Data = new byte[] { (byte)(mMask & 0xFF), (byte)((mMask >> 8) & 0xFF), ValueA, ValueB, ValueC, ValueD, ValueE, ValueF, ValueG, ValueH, ValueI, 0, 0, 0, 0, 0 };
            return cmd;
        }

        public static PSVRReport GetReadDeviceInfo()
        {
            PSVRReport cmd = new PSVRReport();

            cmd.ReportID = 0x81;
            cmd.DataStart = 0xaa;
            cmd.DataLength = 8;
            cmd.Data = new byte[] { 0x80, 0, 0, 0, 0, 0, 0, 0 };

            return cmd;
        }

        public class PSVRDeviceInfoReport
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

            public static PSVRDeviceInfoReport ParseInfo(byte[] Data)
            {
                PSVRDeviceInfoReport info = new PSVRDeviceInfoReport();

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

        public class PSVRDeviceStatusReport
        {
            public PSVRStatusMask Status;
            public uint Volume;
            public ushort UnknownA;
            public byte BridgeOutputID;
            public byte UnknownB;

            public static PSVRDeviceStatusReport ParseStatus(byte[] Data)
            {
                PSVRDeviceStatusReport info = new PSVRDeviceStatusReport();

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
                HeadsetOn = (1 << 0),
                Worn = (1 << 1),
                Cinematic = (1 << 2),
                UnknownA = (1 << 3),
                Headphones = (1 << 4),
                Mute = (1 << 5),
                CEC = (1 << 6),
                UnknownC = (1 << 7),
            }
        }

        public class PSVRUnsolicitedReport
        {
            public byte ReportID;
            public ReportResultCode ResultCode;
            public string Message;

            public static PSVRUnsolicitedReport ParseResponse(byte[] Data)
            {
                PSVRUnsolicitedReport response = new PSVRUnsolicitedReport();

                response.ReportID = Data[0];
                response.ResultCode = (ReportResultCode)Data[1];
                response.Message = Encoding.ASCII.GetString(Data, 2, 58).Replace("\0", "");

                return response;
            }

            public enum ReportResultCode
            {
                Ok,
                UnknownReport,
                UnknownA,
                BadReportLength
            }
        }

        
    };

    public class PSVRSensorEventArgs : EventArgs
    {
        public PSVRSensorReport SensorData { get; set; }
    }

    public class PSVRINEventArgs : EventArgs
    {
        public PSVRReport Response { get; set; }
    }

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
