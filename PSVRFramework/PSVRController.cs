using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSVRFramework
{
    public static class PSVRController
    {
        static object locker = new object();
        static PSVR dev;

        public static PSVR Device { get { return dev; } }

        public static void DeviceConnected(PSVR Device)
        {
            lock (locker)
            {
                if (dev != null)
                {
                    dev.Dispose();
                    dev = null;
                }

                dev = Device;
            }
        }

        public static void DeviceDisconnected()
        {
            lock (locker)
            {
                if (dev != null)
                {
                    dev.Dispose();
                    dev = null;
                }
            }
        }

        public static bool HeadsetOn()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                return dev.SendReport(PSVRReport.GetHeadsetOn());
            }
        }

        public static bool HeadsetOff()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                return dev.SendReport(PSVRReport.GetHeadsetOff());
            }
        }

        public static bool EnableVRTracking()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                return dev.SendReport(PSVRReport.GetEnableVRTracking());
            }
        }

        public static bool EnableVRMode()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                return dev.SendReport(PSVRReport.GetEnterVRMode());
            }
        }

        public static bool EnableCinematicMode()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                return dev.SendReport(PSVRReport.GetExitVRMode());
            }
        }

        public static void Recenter()
        {

            byte current = Settings.Instance.ScreenSize;

            byte fake = 0;
            if (current < 50)
                fake = (byte)(current + 1);
            else
                fake = (byte)(current - 1);

            lock (locker)
            {
                if (dev == null)
                    return;

                Settings.Instance.ScreenSize = fake;
                ApplyCinematicSettings();
                Settings.Instance.ScreenSize = current;
                ApplyCinematicSettings();

                BMI055Integrator.Recenter();
            }
        }

        public static bool Shutdown()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                dev.SendReport(PSVRReport.GetHeadsetOff());
                return dev.SendReport(PSVRReport.GetBoxOff());
            }
        }

        public static bool ApplyCinematicSettings()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                Settings set = Settings.Instance;
               
                var cmd = PSVRReport.GetSetCinematicConfiguration(0xC0, set.ScreenDistance, set.ScreenSize, 0x14, set.Brightness, set.MicVol, false);
                return dev.SendReport(cmd);
            }
        }

        public static bool ApplyLedSettings()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                Settings set = Settings.Instance;
                var cmd = PSVRReport.GetSetHDMLeds(LedMask.All, set.LedAIntensity, set.LedBIntensity, set.LedCIntensity, set.LedDIntensity, set.LedEIntensity, set.LedFIntensity, set.LedGIntensity, set.LedHIntensity, set.LedIIntensity);
                return dev.SendReport(cmd);
            }
        }

        public static bool LedsOn()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                Settings set = Settings.Instance;
                var cmd = PSVRReport.GetSetHDMLeds(LedMask.All, 0x46, 0x46, 0x46, 0x46, 0x46, 0x46, 0x46, 0x46, 0x46);
                return dev.SendReport(cmd);
            }
        }

        public static bool LedsOff()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                Settings set = Settings.Instance;
                var cmd = PSVRReport.GetSetHDMLeds(LedMask.All, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                return dev.SendReport(cmd);
            }
        }

        public static bool LedsDefault()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                Settings set = Settings.Instance;
                var cmd = PSVRReport.GetSetHDMLeds(LedMask.All, 0, 0, 0, 0, 0, 0, 0, 0x46, 0x46);
                return dev.SendReport(cmd);
            }
        }

        public static bool RequestDeviceInfo()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                var cmd = PSVRReport.GetReadDeviceInfo();
                return dev.SendReport(cmd);
            }
        }

        public static bool Raw(byte[] Data)
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
