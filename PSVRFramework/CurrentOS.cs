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
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace PSVRFramework
{
    public static class CurrentOS
    {
        public static bool IsWindows { get; private set; }
        public static bool IsUnix { get; private set; }
        public static bool IsMac { get; private set; }
        public static bool IsLinux { get; private set; }
        public static bool IsUnknown { get; private set; }
        public static bool Is32bit { get; private set; }
        public static bool Is64bit { get; private set; }
        public static bool Is64BitProcess { get { return (IntPtr.Size == 8); } }
        public static bool Is32BitProcess { get { return (IntPtr.Size == 4); } }
        public static string Name { get; private set; }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool wow64Process);

        private static bool Is64bitWindows
        {
            get
            {
                if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) || Environment.OSVersion.Version.Major >= 6)
                {
                    using (Process p = Process.GetCurrentProcess())
                    {
                        bool retVal;
                        if (!IsWow64Process(p.Handle, out retVal)) return false;
                        return retVal;
                    }
                }
                else return false;
            }
        }

        static CurrentOS()
        {
            IsWindows = Path.DirectorySeparatorChar == '\\';
            if (IsWindows)
            {
                Name = Environment.OSVersion.VersionString;

                Name = Name.Replace("Microsoft ", "");
                Name = Name.Replace("  ", " ");
                Name = Name.Replace(" )", ")");
                Name = Name.Trim();

                Name = Name.Replace("NT 6.2", "8 %bit 6.2");
                Name = Name.Replace("NT 6.1", "7 %bit 6.1");
                Name = Name.Replace("NT 6.0", "Vista %bit 6.0");
                Name = Name.Replace("NT 5.", "XP %bit 5.");
                Name = Name.Replace("%bit", (Is64bitWindows ? "64bit" : "32bit"));

                if (Is64bitWindows)
                    Is64bit = true;
                else
                    Is32bit = true;
            }
            else
            {
                string UnixName = ReadProcessOutput("uname");
                if (UnixName.Contains("Darwin"))
                {
                    IsUnix = true;
                    IsMac = true;

                    Name = "MacOS X " + ReadProcessOutput("sw_vers", "-productVersion");
                    Name = Name.Trim();

                    string machine = ReadProcessOutput("uname", "-m");
                    if (machine.Contains("x86_64"))
                        Is64bit = true;
                    else
                        Is32bit = true;

                    Name += " " + (Is32bit ? "32bit" : "64bit");
                }
                else if (UnixName.Contains("Linux"))
                {
                    IsUnix = true;
                    IsLinux = true;

                    Name = ReadProcessOutput("lsb_release", "-d");
                    Name = Name.Substring(Name.IndexOf(":") + 1);
                    Name = Name.Trim();

                    string machine = ReadProcessOutput("uname", "-m");
                    if (machine.Contains("x86_64"))
                        Is64bit = true;
                    else
                        Is32bit = true;

                    Name += " " + (Is32bit ? "32bit" : "64bit");
                }
                else if (UnixName != "")
                {
                    IsUnix = true;
                }
                else
                {
                    IsUnknown = true;
                }
            }
        }

        private static string ReadProcessOutput(string name)
        {
            return ReadProcessOutput(name, null);
        }

        private static string ReadProcessOutput(string name, string args)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                if (args != null && args != "") p.StartInfo.Arguments = " " + args;
                p.StartInfo.FileName = name;
                p.Start();
                // Do not wait for the child process to exit before
                // reading to the end of its redirected stream.
                // p.WaitForExit();
                // Read the output stream first and then wait.
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                if (output == null) output = "";
                output = output.Trim();
                return output;
            }
            catch
            {
                return "";
            }
        }
    }
}