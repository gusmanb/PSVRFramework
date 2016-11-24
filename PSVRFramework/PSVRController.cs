using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSVRFramework
{
    public class PSVRController
    {
        object locker = new object();
        PSVRDevice dev;

        internal PSVRController(PSVRDevice Device)
        {
            dev = Device;
        }

        //internal static PSVRDevice Device { get { return dev; } }

        //internal static void DeviceConnected(PSVRDevice Device)
        //{
        //    lock (locker)
        //    {
        //        if (dev != null)
        //        {
        //            dev.Dispose();
        //            dev = null;
        //        }

        //        dev = Device;
        //    }
        //}

        //internal static void DeviceDisconnected()
        //{
        //    lock (locker)
        //    {
        //        if (dev != null)
        //        {
        //            dev.Dispose();
        //            dev = null;
        //        }
        //    }
        //}

        public bool HeadsetOn()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                return dev.SendReport(PSVRReport.GetHeadsetOn());
            }
        }

        public bool HeadsetOff()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                return dev.SendReport(PSVRReport.GetHeadsetOff());
            }
        }

        public bool EnableVRTracking()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                return dev.SendReport(PSVRReport.GetEnableVRTracking());
            }
        }

        public bool EnableVRMode()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                return dev.SendReport(PSVRReport.GetEnterVRMode());
            }
        }

        public bool EnableCinematicMode()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                return dev.SendReport(PSVRReport.GetExitVRMode());
            }
        }
        
        public bool Shutdown()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                dev.SendReport(PSVRReport.GetHeadsetOff());
                return dev.SendReport(PSVRReport.GetBoxOff());
            }
        }

        public bool ApplyCinematicSettings(byte ScreenDistance, byte ScreenSize, byte Brightness, byte MicFeedback)
        {
            lock (locker)
            {
                if (dev == null)
                    return false;
                
                var cmd = PSVRReport.GetSetCinematicConfiguration(0xC0, ScreenDistance, ScreenSize, 0x14, Brightness, MicFeedback, false);
                return dev.SendReport(cmd);
            }
        }

        public bool ApplyLedSettings(byte[] Values, int Offset)
        {
            lock (locker)
            {

                if (Values == null || Values.Length != 9)
                    return false;

                if (dev == null)
                    return false;
                
                var cmd = PSVRReport.GetSetHDMLeds(LedMask.All, Values[Offset], Values[1 + Offset], Values[2 + Offset], Values[3 + Offset], Values[4 + Offset], Values[5 + Offset], Values[6 + Offset], Values[7 + Offset], Values[8 + Offset]);
                return dev.SendReport(cmd);
            }
        }

        public bool LedsOn()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;
                
                var cmd = PSVRReport.GetSetHDMLeds(LedMask.All, 0x46, 0x46, 0x46, 0x46, 0x46, 0x46, 0x46, 0x46, 0x46);
                return dev.SendReport(cmd);
            }
        }

        public bool LedsOff()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;
                
                var cmd = PSVRReport.GetSetHDMLeds(LedMask.All, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                return dev.SendReport(cmd);
            }
        }

        public bool LedsDefault()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;
                
                var cmd = PSVRReport.GetSetHDMLeds(LedMask.All, 0, 0, 0, 0, 0, 0, 0, 0x46, 0x46);
                return dev.SendReport(cmd);
            }
        }

        public bool RequestDeviceInfo()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                var cmd = PSVRReport.GetReadDeviceInfo();
                return dev.SendReport(cmd);
            }
        }

        public bool ResetPose()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                BMI055Integrator.Recenter();
                return true;
            }
        }

        public bool Recalibrate()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                BMI055Integrator.Recalibrate();
                return true;
            }
        }

        public bool Raw(byte[] Data)
        {
            lock (locker)
            {
                if (dev == null)
                    return false;
                
                return dev.SendRaw(Data);
            }
        }
    }
}
