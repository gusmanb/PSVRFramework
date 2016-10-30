using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibUsbDotNet;

namespace PSVRFramework
{
    public struct PSVRSensor
    {
        //To be analyzed...

        public int volume;
        public bool isWorn;
        public bool isDisplayActive;
        public bool isMute;
        public bool isEarphoneConnected;

        public int MotionX1;
        public int MotionY1;
        public int MotionZ1;

        public int MotionX2;
        public int MotionY2;
        public int MotionZ2;


        public int GyroYaw1;
        public int GyroPitch1;
        public int GyroRoll1;

        public int GyroYaw2;
        public int GyroPitch2;
        public int GyroRoll2;

        //DEBUG
        public int A;
        public int B;
        public int C;
        public int D;
        public int E;
        public int F;
        public int G;
        public int H;
        public int I;
        public int J;
        public int K;
        public int L;
        public int M;
        public int N;
        public int O;


    }

    public struct PSVRState
    {
        public PSVRSensor sensor;
    }

    public class PSVR : IDisposable
    {
        UsbEndpointWriter writer;
        UsbEndpointReader reader;
        IUsbDevice dev;

        public event EventHandler<PSVRSensorEventArgs> SensorDataUpdate;
        public event EventHandler Removed;

        Timer aliveTimer;
        
        public PSVR()
        {
            LibUsbDotNet.Main.UsbDeviceFinder find = new LibUsbDotNet.Main.UsbDeviceFinder(0x054C, 0x09AF);
            var device = LibUsbDotNet.UsbDevice.OpenUsbDevice(find);

            if (device == null)
                throw new InvalidOperationException("No device found");

            dev = (IUsbDevice)device;
            
            for (int ifa = 0; ifa < device.Configs[0].InterfaceInfoList.Count; ifa++)
            {
                var iface = device.Configs[0].InterfaceInfoList[ifa];

                if (iface.Descriptor.InterfaceID == 5)
                {
                    dev.SetConfiguration(1);
                    
                    var res = dev.ClaimInterface(5);
                    res = dev.ClaimInterface(4);

                    writer = dev.OpenEndpointWriter(LibUsbDotNet.Main.WriteEndpointID.Ep04);

                    reader = dev.OpenEndpointReader(LibUsbDotNet.Main.ReadEndpointID.Ep03, 64);
                    reader.DataReceived += Reader_DataReceived;
                    reader.DataReceivedEnabled = true;
                    //reader.
                    //Task.Run(() => 
                    //{

                    //    byte[] buffer = new byte[1];

                    //    while (dev != null)
                    //    {
                    //        int len;

                    //        var data = reader.Read(buffer, 10000, out len);


                    //        if (len != 64)
                    //            continue;

                    //        if (SensorDataUpdate == null)
                    //            return;

                    //        var rep = parse(buffer);

                    //        SensorDataUpdate(this, new PSVRSensorEventArgs { SensorData = rep.sensor });
                    //    }

                    //});

                    aliveTimer = new Timer(is_alive);
                    aliveTimer.Change(2000, 2000);

                    return;
                }
            }

            throw new InvalidOperationException("Device does not match descriptor");
        }
        
        void is_alive(object state)
        {
            if (!dev.UsbRegistryInfo.IsAlive)
            {
                aliveTimer.Change(Timeout.Infinite, Timeout.Infinite);

                if (Removed != null)
                    Removed(this, EventArgs.Empty);

                Dispose();
            }
        }

        private void Reader_DataReceived(object sender, LibUsbDotNet.Main.EndpointDataEventArgs e)
        {
            if (SensorDataUpdate == null)
                return;
            
            var rep = parse(e.Buffer);

            SensorDataUpdate(this, new PSVRSensorEventArgs { SensorData = rep.sensor });
        }

        public bool SendCommand(PSVRCommand Command)
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

        //public static PSVRSensor parseSensor(byte[] data)
        //{

        //    PSVRSensor sensor = new PSVRSensor();
        //    if (data == null)
        //    {
        //        return sensor;
        //    }


        //    /*
        //     * sample data
        //        00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 37 38 39 40 41 42 43 44 45 46 47 48 49 50 51 52 53 54 55 56 57 58 59 60 61 62 63 64
        //        00-00-00-19-00-0A-04-20-00-06-1D-01-FF-7F-00-00-42-AB-D7-69-00-FE-FF-F4-FF-16-00-E1-CF-1F-E0-E1-E9-9F-D9-69-00-FC-FF-EF-FF-24-00-C1-CF-1F-E0-B1-E9-1D-01-00-00-00-03-FF-82-00-00-00-00-00-BC-01-6D
        //        00-00-00-19-00-0A-04-20-00-06-1D-01-FF-7F-00-00-42-93-DB-69-00-FF-FF-FA-FF-13-00-D1-CF-2F-E0-81-E9-88-DD-69-00-FC-FF-0A-00-FC-FF-11-D0-FF-DF-D1-E9-1D-01-00-00-00-03-FF-82-00-00-00-00-00-7D-01-6E
        //        00-00-00-19-00-0A-04-20-00-06-1D-01-FF-7F-00-00-42-7B-DF-69-00-08-00-0E-00-EE-FF-F1-CF-EF-DF-01-EA-70-E1-69-00-04-00-09-00-F9-FF-E1-CF-1F-E0-B1-E9-1D-01-00-00-00-03-FF-82-00-00-00-00-00-84-01-6F

        //        [byte 3]
        //        Volume (0～50)
        //        [byte 9]
        //        0000 0001 Worn (Mounted on Player)
        //        0000 0010 Display is On
        //        0000 0100 ??
        //        0000 1000 Mute
        //        0001 0000 Earphone
        //        0010 0000
        //        0100 0000
        //        1000 0000
        //        reference:
        //        https://github.com/hrl7/node-psvr/blob/master/lib/psvr.js
        //     */


        //    sensor.volume = data[3];

        //    sensor.isWorn = (data[9] & 0x1) == 0x1 ? true : false;//confirmed
        //    sensor.isDisplayActive = (data[9] & 0x2) == 0x2 ? false : true;
        //    sensor.isMute = (data[9] & 0x8) == 0x8 ? true : false;//confirmed

        //    sensor.isEarphoneConnected = (data[9] & 0x10) == 0x10 ? true : false;//confirmed


        //    sensor.MotionX1 = getIntFromInt16(data, 27);
        //    sensor.MotionY1 = getIntFromInt16(data, 29);
        //    sensor.MotionZ1 = getIntFromInt16(data, 31);

        //    sensor.MotionX2 = getIntFromInt16(data, 43);
        //    sensor.MotionY2 = getIntFromInt16(data, 45);
        //    sensor.MotionZ2 = getIntFromInt16(data, 47);

        //    sensor.GyroYaw1 = getIntFromInt16(data, 21);
        //    sensor.GyroPitch1 = getIntFromInt16(data, 23);
        //    sensor.GyroRoll1 = getIntFromInt16(data, 25);

        //    sensor.GyroYaw2 = getIntFromInt16(data, 37);
        //    sensor.GyroPitch2 = getIntFromInt16(data, 39);
        //    sensor.GyroRoll2 = getIntFromInt16(data, 41);


        //    sensor.A = convert(data[19], data[20]);
        //    sensor.B = convert(data[21], data[22]);
        //    sensor.C = convert(data[23], data[24]);

        //    sensor.D = convert(data[25], data[26]);
        //    sensor.E = convert(data[27], data[28]);//下方向加速度
        //    sensor.F = convert(data[29], data[30]);//左方向加速度

        //    sensor.G = convert(data[31], data[32]);//後ろ方向加速度
        //    sensor.H = convert(data[33], data[34]);
        //    sensor.I = convert(data[35], data[36]);

        //    sensor.J = convert(data[37], data[38]);
        //    sensor.K = convert(data[39], data[40]);
        //    sensor.L = convert(data[41], data[42]);

        //    sensor.M = convert(data[43], data[44]);//下方向加速度
        //    sensor.N = convert(data[45], data[46]);//左方向加速度
        //    sensor.O = convert(data[47], data[48]);//後ろ方向加速度


        //    return sensor;

        //}

        public static PSVRSensor parseSensor(byte[] data)
        {

            PSVRSensor sensor = new PSVRSensor();
            if (data == null)
            {
                return sensor;
            }


            /*
             * sample data
                00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 37 38 39 40 41 42 43 44 45 46 47 48 49 50 51 52 53 54 55 56 57 58 59 60 61 62 63 64
                00-00-00-19-00-0A-04-20-00-06-1D-01-FF-7F-00-00-42-AB-D7-69-00-FE-FF-F4-FF-16-00-E1-CF-1F-E0-E1-E9-9F-D9-69-00-FC-FF-EF-FF-24-00-C1-CF-1F-E0-B1-E9-1D-01-00-00-00-03-FF-82-00-00-00-00-00-BC-01-6D
                00-00-00-19-00-0A-04-20-00-06-1D-01-FF-7F-00-00-42-93-DB-69-00-FF-FF-FA-FF-13-00-D1-CF-2F-E0-81-E9-88-DD-69-00-FC-FF-0A-00-FC-FF-11-D0-FF-DF-D1-E9-1D-01-00-00-00-03-FF-82-00-00-00-00-00-7D-01-6E
                00-00-00-19-00-0A-04-20-00-06-1D-01-FF-7F-00-00-42-7B-DF-69-00-08-00-0E-00-EE-FF-F1-CF-EF-DF-01-EA-70-E1-69-00-04-00-09-00-F9-FF-E1-CF-1F-E0-B1-E9-1D-01-00-00-00-03-FF-82-00-00-00-00-00-84-01-6F
            
                [byte 3]
                Volume (0～50)
                [byte 9]
                0000 0001 Worn (Mounted on Player)
                0000 0010 Display is On
                0000 0100 ??
                0000 1000 Mute
                0001 0000 Earphone
                0010 0000
                0100 0000
                1000 0000
                reference:
                https://github.com/hrl7/node-psvr/blob/master/lib/psvr.js
             */


            sensor.volume = data[2];

            sensor.isWorn = (data[8] & 0x1) == 0x1 ? true : false;//confirmed
            sensor.isDisplayActive = (data[8] & 0x2) == 0x2 ? false : true;
            sensor.isMute = (data[8] & 0x8) == 0x8 ? true : false;//confirmed

            sensor.isEarphoneConnected = (data[8] & 0x10) == 0x10 ? true : false;//confirmed


            sensor.MotionX1 = getIntFromInt16(data, 26);
            sensor.MotionY1 = getIntFromInt16(data, 28);
            sensor.MotionZ1 = getIntFromInt16(data, 30);

            sensor.MotionX2 = getIntFromInt16(data, 42);
            sensor.MotionY2 = getIntFromInt16(data, 44);
            sensor.MotionZ2 = getIntFromInt16(data, 46);

            sensor.GyroYaw1 = getIntFromInt16(data, 20);
            sensor.GyroPitch1 = getIntFromInt16(data, 22);
            sensor.GyroRoll1 = getIntFromInt16(data, 24);

            sensor.GyroYaw2 = getIntFromInt16(data, 36);
            sensor.GyroPitch2 = getIntFromInt16(data, 38);
            sensor.GyroRoll2 = getIntFromInt16(data, 40);


            sensor.A = convert(data[18], data[19]);
            sensor.B = convert(data[20], data[21]);
            sensor.C = convert(data[22], data[23]);

            sensor.D = convert(data[24], data[25]);
            sensor.E = convert(data[26], data[27]);//下方向加速度
            sensor.F = convert(data[28], data[29]);//左方向加速度

            sensor.G = convert(data[30], data[31]);//後ろ方向加速度
            sensor.H = convert(data[32], data[33]);
            sensor.I = convert(data[34], data[37]);

            sensor.J = convert(data[36], data[37]);
            sensor.K = convert(data[38], data[39]);
            sensor.L = convert(data[40], data[41]);

            sensor.M = convert(data[42], data[43]);//下方向加速度
            sensor.N = convert(data[44], data[45]);//左方向加速度
            sensor.O = convert(data[46], data[47]);//後ろ方向加速度


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

        public void Dispose()
        {
            aliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            reader.Dispose();
            writer.Dispose();
            try
            {
                dev.Close();
            }
            catch { }

            dev = null;

            SensorDataUpdate = null;
            Removed = null;
        }
    }

    public class PSVRSensorEventArgs : EventArgs
    {
        public PSVRSensor SensorData { get; set; }
    }

    public struct PSVRCommand
    {
        public byte r_id;
        public byte command_status;
        public byte magic;
        public byte length;
        public byte[] data;
        public byte[] Serialize()
        {
            byte[] data = new byte[length + 4];
            data[0] = r_id;
            data[1] = command_status;
            data[2] = magic;
            data[3] = length;

            Buffer.BlockCopy(this.data, 0, data, 4, length);

            return data;
        }

        public static PSVRCommand GetEnableVRTracking()
        {
            PSVRCommand cmd = new PSVRCommand();

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

        public static PSVRCommand GetHeadsetOn()
        {
            PSVRCommand cmd = new PSVRCommand();

            cmd.r_id = 0x17;
            cmd.magic = 0xaa;
            cmd.length = 4;
            byte[] data = new byte[4];
            byte[] tmp = BitConverter.GetBytes(0x00000001);
            Buffer.BlockCopy(tmp, 0, data, 0, 4);
            cmd.data = data;

            return cmd;
        }

        public static PSVRCommand GetHeadsetOff()
        {
            PSVRCommand cmd = new PSVRCommand();

            cmd.r_id = 0x17;
            cmd.magic = 0xaa;
            cmd.length = 4;
            byte[] data = new byte[4];
            byte[] tmp = BitConverter.GetBytes(0x00000000);
            Buffer.BlockCopy(tmp, 0, data, 0, 4);
            cmd.data = data;

            return cmd;
        }

        public static PSVRCommand GetEnterVRMode()
        {
            PSVRCommand cmd = new PSVRCommand();

            cmd.r_id = 0x23;
            cmd.magic = 0xaa;
            cmd.length = 4;
            byte[] data = new byte[4];
            byte[] tmp = BitConverter.GetBytes(0x00000001);
            Buffer.BlockCopy(tmp, 0, data, 0, 4);
            cmd.data = data;

            return cmd;
        }

        public static PSVRCommand GetExitVRMode()
        {
            PSVRCommand cmd = new PSVRCommand();

            cmd.r_id = 0x23;
            cmd.magic = 0xaa;
            cmd.length = 4;
            byte[] data = new byte[4];
            byte[] tmp = BitConverter.GetBytes(0x00000000);
            Buffer.BlockCopy(tmp, 0, data, 0, 4);
            cmd.data = data;

            return cmd;
        }

        public static PSVRCommand GetOff(byte id)
        {
            PSVRCommand cmd = new PSVRCommand();

            cmd.r_id = id;
            cmd.magic = 0xaa;
            cmd.length = 4;
            byte[] data = new byte[4];
            byte[] tmp = BitConverter.GetBytes(0x00000000);
            Buffer.BlockCopy(tmp, 0, data, 0, 4);
            cmd.data = data;

            return cmd;
        }

        public static PSVRCommand GetOn(byte id)
        {
            PSVRCommand cmd = new PSVRCommand();

            cmd.r_id = id;
            cmd.magic = 0xaa;
            cmd.length = 4;
            byte[] data = new byte[4];
            byte[] tmp = BitConverter.GetBytes(0x00000001);
            Buffer.BlockCopy(tmp, 0, data, 0, 4);
            cmd.data = data;

            return cmd;
        }

        public static PSVRCommand GetBoxOff()
        {
            PSVRCommand cmd = new PSVRCommand();

            cmd.r_id = 0x13;
            cmd.magic = 0xaa;
            cmd.length = 4;
            byte[] data = new byte[4];
            byte[] tmp = BitConverter.GetBytes(0x00000001);
            Buffer.BlockCopy(tmp, 0, data, 0, 4);
            cmd.data = data;

            return cmd;
        }


    };
}
