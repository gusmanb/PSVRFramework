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
using System.Windows.Forms;
using Microsoft.Win32;

namespace PSVRToolbox
{
    public static class Utils
    {
        #region Registry functions

        public static bool IsStartupEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {

                    var val = key.GetValue("PSVRToolbox");

                    key.Close();

                    if (val == null)
                        return false;

                    return val.ToString() == Application.ExecutablePath;

                }
            }
            catch { return false; }
        }

        public static bool EnableStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.SetValue("PSVRToolbox", Application.ExecutablePath);
                    key.Close();
                    return true;
                }
            }
            catch { return false; }
        }

        public static bool DisableStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.DeleteValue("PSVRToolbox");
                    return true;
                }
            }
            catch { return false; }
        }

        #endregion

    }

    public static class HiResDateTime
    {
        private static long lastTimeStamp = DateTime.UtcNow.Ticks;
        public static long UtcNowTicks
        {
            get
            {
                long orig, newval;
                do
                {
                    orig = lastTimeStamp;
                    long now = DateTime.UtcNow.Ticks;
                    newval = Math.Max(now, orig + 1);
                } while (Interlocked.CompareExchange
                             (ref lastTimeStamp, newval, orig) != orig);

                return newval;
            }
        }
    }
}
