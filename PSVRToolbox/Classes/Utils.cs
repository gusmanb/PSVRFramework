using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
}
