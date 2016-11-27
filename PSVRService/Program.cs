using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PSVRService
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static int Main(string[] args)
        {
            if (System.Environment.UserInteractive && args.Length > 0)
            {
                // we only care about the first two characters
                string arg = args[0].ToLowerInvariant();

                switch (arg)
                {
                    case "/i":  // install
                        InstallService();
                        return 1;

                    case "/u":  // uninstall
                        UninstallService();
                        return 1;

                    case "/s":
                        if (StartService())
                            Console.WriteLine("Service started");
                        else
                            Console.WriteLine("Cannot start service, ensure it was installed before trying to start it");
                        return 1;

                    case "/p":
                        if (StopService())
                            Console.WriteLine("Service stopped");
                        else
                            Console.WriteLine("Cannot stop service, ensure it was installed");
                        return 1;

                    case "/r":
                        if (RestartService())
                            Console.WriteLine("Service restarted");
                        else
                            Console.WriteLine("Cannot restart service, ensure it was installed before trying to restart it");
                        return 1;

                    default:  // unknown option
                        Console.WriteLine("Argument not recognized: {0}", args[0]);
                        Console.WriteLine(string.Empty);
                        DisplayUsage();
                        return 1;
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new PSVRService()
                };
                ServiceBase.Run(ServicesToRun);
            }

            return 1;
            
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("Usage mode: PSVRService [OPTIONS]");
            Console.WriteLine("");
            Console.WriteLine("If executed interactively, the service can be self installed/uninstalled.");
            Console.WriteLine("Possible parameters are:");
            Console.WriteLine("");
            Console.WriteLine(" /I     Installs and starts the service on the system");
            Console.WriteLine(" /U     Stops and removes the service from the system");
            Console.WriteLine(" /S     Starts the service");
            Console.WriteLine(" /P     Stops the service");
            Console.WriteLine(" /R     Restarts the service");
            Console.WriteLine("");
            Console.WriteLine("All these commands need to be executed with administrator privileges.");
            Console.WriteLine("These commands work only on Windows, on Linux you must control the service with mono-service.");
        }

        private static int InstallService()
        {
            try
            {
                // install the service with the Windows Service Control Manager (SCM)
                ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                if (!StartService())
                    Console.WriteLine("Service was installed but not started, restart your computer to apply these changes");
                else
                    Console.WriteLine("Service successfully installed");

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType() == typeof(Win32Exception))
                {
                    Win32Exception wex = (Win32Exception)ex.InnerException;
                    Console.WriteLine("Error(0x{0:X}): Service already installed!", wex.ErrorCode);
                    return wex.ErrorCode;
                }
                else
                {
                    Console.WriteLine(ex.ToString());
                    return -1;
                }
            }

            return 0;
        }

        private static int UninstallService()
        {
            try
            {
                StopService();
                // uninstall the service from the Windows Service Control Manager (SCM)
                ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
            }
            catch (Exception ex)
            {
                if (ex.InnerException.GetType() == typeof(Win32Exception))
                {
                    Win32Exception wex = (Win32Exception)ex.InnerException;
                    Console.WriteLine("Error(0x{0:X}): Service not installed!", wex.ErrorCode);
                    return wex.ErrorCode;
                }
                else
                {
                    Console.WriteLine(ex.ToString());
                    return -1;
                }
            }

            return 0;
        }

        private static bool StartService()
        {
            ServiceController srv = new ServiceController("PSVRServer");

            if (srv == null)
                return false;

            srv.Start();
            return true;
        }

        private static bool StopService()
        {
            ServiceController srv = new ServiceController("PSVRServer");

            if (srv == null)
                return false;

            srv.Stop();
            return true;
        }

        private static bool RestartService()
        {
            ServiceController srv = new ServiceController("PSVRServer");

            if (srv == null)
                return false;

            srv.Stop();
            srv.WaitForStatus(ServiceControllerStatus.Stopped);
            srv.Start();
            return true;
        }
    }
}
