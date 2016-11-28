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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSVRToolbox
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                RemoteCommand cmd = null;
                switch (args[0])
                {
                    case "CinematicSettings":

                        CinematicSettingsCommand ccmd = new CinematicSettingsCommand();

                        foreach (var cmdParam in args.Skip(1))
                        {
                            string[] parts = cmdParam.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length != 2)
                                continue;

                            string param = parts[0];
                            byte value;

                            if (!byte.TryParse(parts[1], out value))
                                continue;

                            switch (param)
                            {
                                case "Size":
                                    ccmd.Size = value;
                                    break;
                                case "Distance":
                                    ccmd.Distance = value;
                                    break;
                                case "Brightness":
                                    ccmd.Brightness = value;
                                    break;
                            }
                        }

                        ccmd.Command = "CinematicSettings";
                        cmd = ccmd;

                        break;

                    case "LedSettings":

                        LEDSettingsCommand lcmd = new LEDSettingsCommand();

                        foreach (var cmdParam in args.Skip(1))
                        {
                            string[] parts = cmdParam.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length != 2)
                                continue;

                            string param = parts[0];
                            byte value;

                            if (!byte.TryParse(parts[1], out value))
                                continue;

                            switch (param)
                            {
                                case "LedA":
                                    lcmd.LedA = value;
                                    break;
                                case "LedB":
                                    lcmd.LedB = value;
                                    break;
                                case "LedC":
                                    lcmd.LedC = value;
                                    break;
                                case "LedD":
                                    lcmd.LedD = value;
                                    break;
                                case "LedE":
                                    lcmd.LedE = value;
                                    break;
                                case "LedF":
                                    lcmd.LedF = value;
                                    break;
                                case "LedG":
                                    lcmd.LedG = value;
                                    break;
                                case "LedH":
                                    lcmd.LedH = value;
                                    break;
                                case "LedI":
                                    lcmd.LedI = value;
                                    break;
                            }
                        }

                        lcmd.Command = "LedSettings";
                        cmd = lcmd;

                        break;

                    default:

                        cmd = new RemoteCommand { Command = args[0] };
                        break;
                }

                if (cmd != null)
                {
                    string ser = JsonConvert.SerializeObject(cmd);
                    byte[] data = Encoding.UTF8.GetBytes(ser);


                    var ep = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 14598);
                    var client = new UdpClient();
                    client.EnableBroadcast = true;

                    client.Send(data, data.Length, ep);
                    client.Close();
                }

                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

    }
    
}
