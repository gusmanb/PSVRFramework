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

        public bool Recenter(int CurrentDistance)
        {
            lock (locker)
            {
                if (dev == null)
                    return false;
                
                return true;
            }
        }

        public bool RecalibrateDevice()
        {
            lock (locker)
            {
                if (dev == null)
                    return false;

                BMI055Integrator.Recalibrate();
                return true;
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
