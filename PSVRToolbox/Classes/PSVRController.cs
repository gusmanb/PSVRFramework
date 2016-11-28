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
