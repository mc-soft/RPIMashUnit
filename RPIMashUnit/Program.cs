using System;
using System.IO;
using System.Threading;
using RPIMashUnit.Core;
using RPIMashUnit.Helper;

namespace RPIMashUnit
{
    class Program
    {
        private static bool _isBoot;
        private static bool _isBackground;
        
        // TODO: Implement
        // TODO: If settings.cfg is changed, core.settings needs to be deleted (ADD TO DOCUMENTATION!!)
        static void Main(string[] args)
        {
            // Check for command-line args and set the requried variables.
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

                    case "background":
                        _isBackground = true;
                        break;
                }
            }

            try
            {
                // Display ASCII header and credits/version.
                Log.DisplayHeader();

                // Loop to check the network is online before continuing further.
                while (!Network.IsOnline()) Thread.Sleep(60000);
                
                // Initialize core classes.
                Devices devices = new Devices();
                Settings settings = InitializeSettings();

                // If this is the software boot reset the ReportStatusEpoch to 0 so a [BOOT] status report is emailed.
                if (_isBoot)
                    settings.ResetReportStatusEpoch();

                Log.DebugWriteLine("Devices/Settings classes have been initialized successfully.");

                do
                {
                    try
                    {
                        // If not currently online skip remaining loop body until we are.
                        if (!Network.IsOnline()) continue;

                        // Core task method, handles the changing of passwords/notification emails.
                        DoTasks(settings, devices, _isBoot);
                        
                        if (_isBoot)
                        {
                            if (settings.CurrentPassword != null && Time.GetCurrentEpoch() < settings.PasswordChangeEpoch)
                            {
                                var e = new SendMail(settings.MailToPassword);
                                e.SendPasswordNotice(settings.CurrentPassword, Time.EpochToTimestamp(settings.PasswordChangeEpoch));
                            }

                            _isBoot = false;
                        }

                        // Sleep for a minute before relooping.
                        Thread.Sleep(60 * 1000);
                    }
                    catch (Exception ex)
                    {
                        // Log any exceptions that occur for debugging purposes.
                        ErrorHandler.LogError(ex);

                        // Update the device status incase the exception is due to a parse error.
                        devices.UpdateDeviceStatus();

                        // Sleep or 2 minutes before restarting the loop.
                        Thread.Sleep(120 * 1000);
                    }
                } while (true);
            }
            catch (Exception ex)
            {
                Log.ErrorWriteLine("Failed initializing devices/settings classes, stopping execution.");

                // Log any exceptions that occur for debugging purposes.
                ErrorHandler.LogError(ex);
            }
        }

        /// <summary>
        /// Core method that handles the all password changing and sending of notification emails at the correct times.
        /// </summary>
        /// <param name="settings">Settings class containing all required settings and parameters.</param>
        /// <param name="devices">Devices class that contains required device methods/information.</param>
        /// <param name="isBoot">Boolean value that indicates if this is the softwares initial launch.</param>
        private static void DoTasks(Settings settings, Devices devices, bool isBoot)
        {
            if (settings.ReportStatusEpoch == 0)
            {
                // Device status does not appear to have been reported yet, report and update timings.
                ReportDeviceStatus(devices, settings, isBoot);
            }
            else if (settings.PasswordChangeEpoch == 0)
            {
                // Password does not appear to have been changed yet, change password and update timings.
                settings.CreateFirstPassword();
            }
            else if (Time.GetCurrentEpoch() >= settings.PasswordChangeEpoch)
            {
                // It is time for the password change to take place.
                settings.UpdatePassword();
            }
            else if (Time.GetCurrentEpoch() >= settings.PasswordPrewarnEpoch && !settings.PasswordHasWarned)
            {
                // It is time for the next password to be generated and a notification email sent out.
                settings.CreateNextPassword();
            }
            else if (Time.GetCurrentEpoch() >= settings.ReportStatusEpoch)
            {
                // It is time to report the status of all devices via email.
                ReportDeviceStatus(devices, settings, isBoot);
            }
        }

        /// <summary>
        /// Check the current status of all devices and report this via email.
        /// </summary>
        /// <param name="devices">Devices class that contains required device methods/information.</param>
        /// <param name="settings">Settings class containing all required settings and parameters.</param>
        /// <param name="isBoot">Boolean value that indicates if this is the softwares initial launch.</param>
        private static void ReportDeviceStatus(Devices devices, Settings settings, bool isBoot)
        {
            // Gets the status of all devices at the present moment in time.
            devices.UpdateDeviceStatus();

            // Send a boot/status email containing the updated information.
            var e = new SendMail(settings.MailToStatus);
            e.SendStatusEmail(devices, settings, isBoot);

            // If this is the initial launch/boot, save the DateTime for future use.
            if (isBoot)
                File.WriteAllText("lastboot.dat", DateTime.Now.ToString("dd/MM/yyyy - HH:mm"));

            // Calculate a new Epoch that will be used to determine the time of next device status report.
            settings.ReportStatusEpoch =
                Time.CalculateReportStatusEpoch(settings.ReportFrequency, settings.FrequencyMultiplier);

            // Serialize the settings file containing updated Epoch to file.
            Serializer.Serialize(settings);
        }

        /// <summary>
        /// Initializes the settings file by either creating a new instance (if first launch) or deserializing the previous one from a file.
        /// </summary>
        /// <returns>A new instance of our settings class if first launch, otherwise our previous previous instance containing all our required settings.</returns>
        private static Settings InitializeSettings()
        {
            if (File.Exists("core.settings"))
            {
                return Serializer.Deserialize();
            }

            return new Settings();
        }
    }
}
