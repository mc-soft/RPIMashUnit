using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RPIMashUnit.Core;
using RPIMashUnit.Helper;

namespace RPIMashUnit
{
    class Program
    {
        // Upon Boot Up the PI should establish the status of the various components in the system, Ruckus Transmitter, Cisco Switch, UPS, and the other PI. 
        // Upon Boot Up, PI should actively seek access to the internet (locating Google’s 8.8.8.8)
        // Once internet access is gained, an email should be sent to Lde informing of system start up and status of all components, (it should also keep a log of the last time it was active and report this also) 
        // Once active it should continue to monitor the system and report on a regular basis as at present. 
        private static bool _isBoot;

        // TODO: Add little ASCII art/about me information.
        // TODO: If settings.cfg is changed, core.settings needs to be deleted (ADD TO DOCUMENTATION!!)
        // TODO: Update devices before sending device status email.
        static void Main(string[] args)
        {
            try
            {
                // Check for command line args and set the requried variables.
                foreach (string arg in args)
                {
                    switch (arg)
                    {
                        case "boot":
                            _isBoot = true;
                            break;

                        case "debug":
                            Log.IsDebug = true;
                            break;
                    }
                }

                var devices = new Devices();
                var settings = InitializeSettings();

                // Main loop.
                while (true)
                {
                    if (!Network.IsOnline()) continue;

                    if (!_isBoot)
                        devices.UpdateDeviceStatus();

                    ReportDeviceStatus(devices, settings, _isBoot);
                    CheckPassword(settings);

                    // Update Ruckus host incase the address has changed.
                    settings.RuckusHost = devices.GetIpFromMac(settings.RuckusMac);

                    if (_isBoot)
                        _isBoot = false;

                    Thread.Sleep(settings.SleepTime);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex);
                Main(args);
            }
        }

        private static void ReportDeviceStatus(Devices devices, Settings settings, bool isBoot)
        {
            var e = new SendMail("mash@lde.co.uk");
            e.SendStatusEmail(devices, settings, isBoot);

            if (isBoot)
                File.WriteAllText("lastboot.dat", DateTime.Now.ToString("dd/MM/yyyy - HH:mm"));
        }

        private static Settings InitializeSettings()
        {
            if (File.Exists("core.settings"))
            {
                return Serializer.Deserialize<Settings>();
            }

            return new Settings();
        }

        private static void CheckPassword(Settings settings)
        {
            if (settings.PasswordChangeEpoch == 0)
            {
                // First run, no password currently created.
                settings.CreateFirstPassword();
            }
            else if (Time.GetCurrentEpoch() >= settings.PasswordChangeEpoch)
            {
                // Change password and send email notification.
                settings.UpdatePassword();
            }
            else if (Time.GetCurrentEpoch() >= settings.PasswordPrewarnEpoch && !settings.HasWarned)
            {
                // Password will be changing soon, send warning email notification.
                settings.CreateNextPassword();
            }
        }
    }
}
