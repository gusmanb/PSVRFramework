using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSVRFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSVRToolbox
{
    public class RemoteCommandListener : IDisposable
    {
        UdpClient client;
        public RemoteCommandListener(int Port)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, Port);

            client = new UdpClient();
            client.EnableBroadcast = true;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.ExclusiveAddressUse = false;
            client.Client.Bind(ep);

            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        byte[] data = client.Receive(ref ep);
                        string sData = Encoding.UTF8.GetString(data);

                        JToken obj = (JToken)JsonConvert.DeserializeObject(sData);

                        RemoteCommand cmd = null;
                        string scmd = obj.Value<string>("Command").ToString();

                        if (scmd == "CinematicSettings")
                            cmd = obj.ToObject<CinematicSettingsCommand>();
                        else if (scmd == "LedSettings")
                            cmd = obj.ToObject<LEDSettingsCommand>();
                        else
                            cmd = obj.ToObject<RemoteCommand>();
                        
                        if (cmd != null && !string.IsNullOrWhiteSpace(cmd.Command))
                            ProcessCommand(cmd);
                    }
                }
                catch { }
            });
        }

        private void ProcessCommand(RemoteCommand cmd)
        {
            switch (cmd.Command)
            {
                case "HeadsetOn":
                    PSVRController.HeadsetOn();
                    break;
                case "HeadsetOff":
                    PSVRController.HeadsetOff();
                    break;
                case "EnableVRTracking":
                    PSVRController.EnableVRTracking();
                    break;
                case "EnableVRMode":
                    PSVRController.EnableVRMode();
                    break;
                case "EnableCinematicMode":
                    PSVRController.EnableCinematicMode();
                    break;
                case "Recenter":

                    byte current = Settings.Instance.ScreenSize;

                    byte fake = 0;
                    if (current < 50)
                        fake = (byte)(current + 1);
                    else
                        fake = (byte)(current - 1);

                    PSVRController.ApplyCinematicSettings(Settings.Instance.ScreenDistance, fake, Settings.Instance.Brightness, Settings.Instance.MicVol);
                    Thread.Sleep(100);
                    PSVRController.ApplyCinematicSettings(Settings.Instance.ScreenDistance, current, Settings.Instance.Brightness, Settings.Instance.MicVol);
                    break;
                case "Shutdown":
                    PSVRController.Shutdown();
                    break;
                case "CinematicSettings":

                    CinematicSettingsCommand ccmd = cmd as CinematicSettingsCommand;

                    if (ccmd != null)
                    {
                        bool apply = false;

                        if (ccmd.Brightness != null && ccmd.Brightness.HasValue)
                        {
                            var bright = ccmd.Brightness.Value;

                            if (bright > 32)
                                return;

                            Settings.Instance.Brightness = bright;
                            apply = true;
                        }

                        if (ccmd.Size != null && ccmd.Size.HasValue)
                        {
                            var siz = ccmd.Size.Value + 26;

                            if (siz > 80)
                            {
                                Settings.ReadSettings();
                                return;
                            }

                            Settings.Instance.ScreenSize = (byte)siz;
                            apply = true;
                        }

                        if (ccmd.Distance != null && ccmd.Distance.HasValue)
                        {
                            var dist = ccmd.Distance.Value + 2;

                            if (dist > 50)
                            {
                                Settings.ReadSettings();
                                return;
                            }

                            Settings.Instance.ScreenDistance = (byte)dist;
                            apply = true;
                        }

                        if (apply)
                            PSVRController.ApplyCinematicSettings(Settings.Instance.ScreenDistance, Settings.Instance.ScreenSize, Settings.Instance.Brightness, Settings.Instance.MicVol);
                    }

                    break;

                case "LedSettings":

                    LEDSettingsCommand lcmd = cmd as LEDSettingsCommand;

                    if (lcmd != null)
                    {
                        bool apply = false;

                        if (lcmd.LedA != null && lcmd.LedA.HasValue)
                        {
                            if (lcmd.LedA.Value > 100)
                                return;

                            Settings.Instance.LedAIntensity = lcmd.LedA.Value;
                            apply = true;
                        }

                        if (lcmd.LedB != null && lcmd.LedB.HasValue)
                        {
                            if (lcmd.LedB.Value > 100)
                            {
                                Settings.ReadSettings();
                                return;
                            }
                            Settings.Instance.LedBIntensity = lcmd.LedB.Value;
                            apply = true;
                        }

                        if (lcmd.LedC != null && lcmd.LedC.HasValue)
                        {
                            if (lcmd.LedC.Value > 100)
                            {
                                Settings.ReadSettings();
                                return;
                            }
                            Settings.Instance.LedCIntensity = lcmd.LedC.Value;
                            apply = true;
                        }

                        if (lcmd.LedD != null && lcmd.LedD.HasValue)
                        {
                            if (lcmd.LedD.Value > 100)
                            {
                                Settings.ReadSettings();
                                return;
                            }
                            Settings.Instance.LedDIntensity = lcmd.LedD.Value;
                            apply = true;
                        }

                        if (lcmd.LedE != null && lcmd.LedE.HasValue)
                        {
                            if (lcmd.LedE.Value > 100)
                            {
                                Settings.ReadSettings();
                                return;
                            }
                            Settings.Instance.LedEIntensity = lcmd.LedE.Value;
                            apply = true;
                        }

                        if (lcmd.LedF != null && lcmd.LedF.HasValue)
                        {
                            if (lcmd.LedF.Value > 100)
                            {
                                Settings.ReadSettings();
                                return;
                            }
                            Settings.Instance.LedFIntensity = lcmd.LedF.Value;
                            apply = true;
                        }

                        if (lcmd.LedG != null && lcmd.LedG.HasValue)
                        {
                            if (lcmd.LedG.Value > 100)
                            {
                                Settings.ReadSettings();
                                return;
                            }
                            Settings.Instance.LedGIntensity = lcmd.LedG.Value;
                            apply = true;
                        }

                        if (lcmd.LedH != null && lcmd.LedH.HasValue)
                        {
                            if (lcmd.LedH.Value > 100)
                            {
                                Settings.ReadSettings();
                                return;
                            }
                            Settings.Instance.LedHIntensity = lcmd.LedH.Value;
                            apply = true;
                        }

                        if (lcmd.LedI != null && lcmd.LedI.HasValue)
                        {
                            if (lcmd.LedI.Value > 100)
                            {
                                Settings.ReadSettings();
                                return;
                            }
                            Settings.Instance.LedIIntensity = lcmd.LedI.Value;
                            apply = true;
                        }

                        if (apply)
                            PSVRController.ApplyLedSettings();
                    }

                    break;
                case "StoreSettings":
                    Settings.SaveSettings(); 
                    break;
                case "DiscardSettings":
                    Settings.ReadSettings();
                    break;
                    
            }
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }
        }
  
    }

    public class RemoteCommand
    {
        public string Command { get; set; }
    }

    public class CinematicSettingsCommand : RemoteCommand
    {
        public byte? Distance { get; set; }
        public byte? Size { get; set; }
        public byte? Brightness { get; set; }
    }

    public class LEDSettingsCommand : RemoteCommand
    {
        public byte? LedA { get; set; }
        public byte? LedB { get; set; }
        public byte? LedC { get; set; }
        public byte? LedD { get; set; }
        public byte? LedE { get; set; }
        public byte? LedF { get; set; }
        public byte? LedG { get; set; }
        public byte? LedH { get; set; }
        public byte? LedI { get; set; }
    }
}
