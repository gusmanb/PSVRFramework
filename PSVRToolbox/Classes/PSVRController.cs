using PSVRFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSVRToolbox
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

        public static void HeadsetOn()
        {
            lock (locker)
            {
                if (dev == null)
                    return;

                dev.SendCommand(PSVRMessage.GetHeadsetOn());
            }
        }

        public static void HeadsetOff()
        {
            lock (locker)
            {
                if (dev == null)
                    return;

                dev.SendCommand(PSVRMessage.GetHeadsetOff());
            }
        }

        public static void EnableVRTracking()
        {
            lock (locker)
            {
                if (dev == null)
                    return;

                dev.SendCommand(PSVRMessage.GetEnableVRTracking());
                dev.SendCommand(PSVRMessage.GetEnterVRMode());
            }
        }

        public static void EnableVRMode()
        {
            lock (locker)
            {
                if (dev == null)
                    return;

                dev.SendCommand(PSVRMessage.GetEnterVRMode());
            }
        }

        public static void EnableCinematicMode()
        {
            lock (locker)
            {
                if (dev == null)
                    return;

                dev.SendCommand(PSVRMessage.GetExitVRMode());
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
            }
        }

        public static void Shutdown()
        {
            lock (locker)
            {
                if (dev == null)
                    return;

                dev.SendCommand(PSVRMessage.GetHeadsetOff());
                dev.SendCommand(PSVRMessage.GetBoxOff());
            }
        }

        public static void ApplyCinematicSettings()
        {
            lock (locker)
            {
                if (dev == null)
                    return;

                Settings set = Settings.Instance;
               
                var cmd = PSVRMessage.GetSetCinematicConfiguration(0xC0, set.ScreenDistance, set.ScreenSize, 0x14, set.Brightness, set.MicVol, false);
                dev.SendCommand(cmd);
            }
        }

        public static void ApplyLedSettings()
        {
            lock (locker)
            {
                if (dev == null)
                    return;

                Settings set = Settings.Instance;
                var cmd = PSVRMessage.GetSetHDMLeds(LedMask.All, set.LedAIntensity, set.LedBIntensity, set.LedCIntensity, set.LedDIntensity, set.LedEIntensity, set.LedFIntensity, set.LedGIntensity, set.LedHIntensity, set.LedIIntensity);
                dev.SendCommand(cmd);
            }
        }

        public static void RequestDeviceInfo()
        {
            lock (locker)
            {
                if (dev == null)
                    return;

                var cmd = PSVRMessage.GetReadDeviceInfo();
                dev.SendCommand(cmd);
            }
        }
    }
}
