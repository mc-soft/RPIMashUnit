using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using RPIMashUnit.Core;

namespace RPIMashUnit.Helper
{
    internal class SendMail
    {
        private SmtpClient EmailClient { get; }

        private string MailTo { get; }
        private const string MailFrom = "lde.dvr@lde.co.uk";

        public SendMail(string mailTo)
        {
            MailTo = mailTo;

            const string smtpUser = "lde.dvr@lde.co.uk";
            const string smtpPass = "dvr";

            // Create SMTP client using our mailserver details.
            EmailClient = new SmtpClient
            {
                Port = 25,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                Host = "serge.lde.co.uk"
            };
        }

        /// <summary>
        /// Sends a status email containing boot times and device information.
        /// </summary>
        /// <param name="devices">Devices class that contains required device methods/information.</param>
        /// <param name="settings">Settings class containing all required settings and parameters.</param>
        /// <param name="isBoot">Boolean value that indicates if this is the softwares initial launch.</param>
        /// <returns>True if the email sent successfully or false if otherwise.</returns>
        public bool SendStatusEmail(Devices devices, Settings settings, bool isBoot)
        {
            Log.DebugWriteLine(string.Format("Sending status email to {0}, [BOOT={1}]", MailTo, isBoot));

            try
            {
                string currentDatetime = DateTime.Now.ToString("dd/MM/yyyy - HH:mm");

                // Initialize a StringBuilder that will be used as the body of the email.
                StringBuilder body = new StringBuilder();
                body.AppendLine(string.Format("Current time:\t{0}", currentDatetime));
                body.AppendLine(string.Format("Last boot:\t\t{0}", settings.LastBootTime ?? "N/A (This appears to be the first boot)"));
                body.AppendLine("\n[DEVICES]:\n");

                // Iterate through devices and output the information for each device.
                foreach (var device in devices.DeviceList)
                {
                    body.AppendLine(string.Format("{0} ({1}) is {2}",
                        device.Name,
                        device.IP,
                        device.IsOnline ? "online" : "OFFLINE"));
                }

                // If the current password is set, display it.
                if (settings.CurrentPassword != null)
                    body.AppendLine(string.Format("The current password is {0}", settings.CurrentPassword));

                // Initializes the MailMessage containing the subject/body.
                var mail = new MailMessage(MailFrom, MailTo)
                {
                    Subject = isBoot
                                ? string.Format("[BOOT] {0} has booted at {1}", settings.CurrentDeviceName, currentDatetime)
                                : string.Format("[MASH] {0} status report", settings.CurrentDeviceName),

                    Body = body.ToString()
                };

                // Sends our email using the SMTP client configured in the constructor.
                EmailClient.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                // Logs the exception.
                ErrorHandler.LogError(ex);

                return false;
            }
        }

        /// <summary>
        /// Sends a notification email about a recent password change.
        /// </summary>
        /// <param name="password">The new password that is currently in use.</param>
        /// <param name="changeDate">The date & time of the password change.</param>
        /// <returns>True if the email sent successfully or false if otherwise.</returns>
        public bool SendPasswordUpdate(string password, string changeDate)
        {
            Log.DebugWriteLine(string.Format("Password changed to {0}, next update at {1} - sending email.", password, changeDate));

            try
            {
                string currentDatetime = DateTime.Now.ToString("dd/MM/yyyy - HH:mm");

                // Initialize a StringBuilder that will be used as the body of the email.
                StringBuilder body = new StringBuilder();
                body.AppendLine(string.Format("The public password has been changed to: {0}\n", password));
                body.AppendLine(string.Format("Time of change:\t{0}", currentDatetime));
                body.AppendLine(string.Format("The next change will occur at ~{0}", changeDate));

                // Initializes the MailMessage containing the subject/body.
                var mail = new MailMessage(MailFrom, MailTo)
                {
                    Subject = string.Format("[PASS] The public password has been changed to {0}", password),
                    Body = body.ToString()
                };

                // Sends our email using the SMTP client configured in the constructor.
                EmailClient.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                // Logs the exception.
                ErrorHandler.LogError(ex);

                return false;
            }
        }

        public void SendPasswordNotice(string password, string changeDate)
        {
            Log.DebugWriteLine("The settings file already exists, sending the current password via email.");

            try
            {
                string currentDatetime = DateTime.Now.ToString("dd/MM/yyyy - HH:mm");

                StringBuilder body = new StringBuilder();
                body.AppendLine(string.Format("The RPIMash unit has booted at {0}\n", currentDatetime));
                body.AppendLine(string.Format("The current public password is: {0}", password));
                body.AppendLine(string.Format("The next change will occur at ~{0}", changeDate));

                var mail = new MailMessage(MailFrom, MailTo)
                {
                    Subject = string.Format("[PASS] Public Ruckus password information"),
                    Body = body.ToString()
                };

                EmailClient.Send(mail);
            }
            catch (Exception e)
            {
                ErrorHandler.LogError(e);
            }
        }

        /// <summary>
        /// Sends a notification email about an upcoming password change.
        /// </summary>
        /// <param name="password">The password that is going to be used in the upcoming change.</param>
        /// <param name="changeDate">The date & time that the password will be changed.</param>
        /// <returns>True if the email sent successfully or false if otherwise.</returns>
        public bool SendPasswordWarning(string password, string changeDate)
        {
            Log.DebugWriteLine(string.Format("Password changing to {0} at {1} - sending email.", password, changeDate));

            try
            {
                // Initialize a StringBuilder that will be used as the body of the email.
                StringBuilder body = new StringBuilder();
                body.AppendLine("The public password will soon be changed, below are the new details you will require:\n");
                body.AppendLine(string.Format("New Password: {0}", password));
                body.AppendLine(string.Format("This password will be in effect at ~{0}", changeDate));

                // Initializes the MailMessage containing the subject/body.
                var mail = new MailMessage(MailFrom, MailTo)
                {
                    Subject = string.Format("[PASS] The public password will be changing at {0}", changeDate),
                    Body = body.ToString()
                };

                // Sends our email using the SMTP client configured in the constructor.
                EmailClient.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex);

                return false;
            }
        }

        /// <summary>
        /// Sends an email containing information about a recent exception.
        /// </summary>
        /// <param name="errorLog">The errorlog of the exception.</param>
        public void SendExceptionDetails(StringBuilder errorLog)
        {
            Log.DebugWriteLine("Sending email containing exception information for debugging.");

            // Initializes the MailMessage containing the subject/body.
            var mail = new MailMessage(MailFrom, MailTo)
            {
                Subject = "[ERROR] An exception has occured!",
                Body = errorLog.ToString()
            };

            // Sends our email using the SMTP client configured in the constructor.
            EmailClient.Send(mail);
        }
    }
}
