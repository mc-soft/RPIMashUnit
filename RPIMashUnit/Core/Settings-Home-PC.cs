using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RPIMashUnit.Helper;

namespace RPIMashUnit.Core
{
    public class Settings
    {
        private readonly Random R = new Random();

        enum Multiplier
        {
            Days = 86400,
            Hours = 3600,
            Minutes = 60
        };

        public string DeviceName { get; set; }
        public string MailToStatus { get; set; }
        public string MailToPass { get; set; }
        public string LastBootTime { get; set; }

        public int SleepTime { get; set; }
        public int PasswordChangeEpoch { get; set; }
        public int PasswordPrewarnEpoch { get; set; }
        public bool HasWarned { get; set; }

        public const int PasswordMultipler = (int)Multiplier.Minutes;
        public int PasswordChangeFrequency { get; set; } // The number of days/hours/mins the password should be changed.
        public int PasswordPrewarnTime { get; set; } // The number of days/hours/mins before the password change the warning email should be sent.

        public string CurrentPassword { get; set; }
        public string NextPassword { get; set; }
        public string RuckusHost { get; set; }
        public string RuckusMac { get; set; }
        
        public Settings()
        {
            const string settingsConfig = "settings.cfg";
            const string lastBootFile = "lastboot.dat";

            if (!File.Exists(settingsConfig))
                throw new FileNotFoundException(string.Format("Cannot find {0}", settingsConfig));

            if (File.Exists(lastBootFile))
                LastBootTime = File.ReadAllText(lastBootFile);

            foreach (string line in File.ReadAllLines(settingsConfig).Where(s => s.Length > 0 && s[0] != '#'))
            {
                var split = line.Split('=');
                string setting = split[0];
                string value = split[1];

                switch (setting)
                {
                    case "devicename":
                        DeviceName = value;
                        break;

                    case "rksmac":
                        RuckusMac = value;
                        break;

                    case "sleeptime":
                        SleepTime = Convert.ToInt32(value) * 60 * 1000;
                        break;

                    case "mailtostatus":
                        MailToStatus = value;
                        break;

                    case "mailtopass":
                        MailToPass = value;
                        break;

                    case "passchangetime":
                        PasswordChangeFrequency = Convert.ToInt32(value);
                        break;

                    case "passwarningtime":
                        PasswordPrewarnTime = Convert.ToInt32(value);
                        break;
                }
            }
        }

        /// <summary>
        /// Generate and set the first password on first-time execution.
        /// </summary>
        public void CreateFirstPassword()
        {
            Log.DebugWriteLine("First run detected, creating initial password...");

            NextPassword = GeneratePassword();
            UpdatePassword();
        }

        /// <summary>
        /// Generates the next password that will be used and emails out a notification letting people know of the upcoming change.
        /// </summary>
        public void CreateNextPassword()
        {
            NextPassword = GeneratePassword();

            Log.DebugWriteLine(string.Format("Next password generated, it will be {0}.", NextPassword));

            var e = new SendMail(MailToPass);

            while (!e.SendPasswordWarning(NextPassword, Time.EpochToTimestamp(PasswordChangeEpoch)))
            {
                Log.ErrorWriteLine("Failure sending password change email, trying again in 5 minutes.");
                Thread.Sleep(60 * 5 * 1000); // Sleep for 5 minutes and then try again.
            }

            HasWarned = true;
            Serializer.Serialize(this);
        }

        /// <summary>
        /// Changes the previously generated password to the current password and emails out a notification letting people know of the new change.
        /// </summary>
        public void UpdatePassword()
        {
            CurrentPassword = NextPassword;

            Ruckus rks = new Ruckus();
            while (!rks.ChangePassword(CurrentPassword, RuckusHost))
            {
                Log.ErrorWriteLine("Failed to change password, trying again in 5 minutes.");
                Thread.Sleep(60 * 5 * 1000); // Sleep for 5 minutes and then try again.
            }

            UpdateEpochs();
            string nextPasswordChangeTimestamp = Time.EpochToTimestamp(PasswordChangeEpoch);

            var e = new SendMail(MailToPass);
            while (!e.SendPasswordUpdate(CurrentPassword, nextPasswordChangeTimestamp))
            {
                Log.ErrorWriteLine("Failed to send new password email notification, trying again in 5 minutes.");
                Thread.Sleep(60 * 5 * 1000); // Sleep for 5 minutes and then try again.
            }

            Log.DebugWriteLine(string.Format("Password successfully changed to {0}", CurrentPassword));
            Log.DebugWriteLine(string.Format("The password will next be changed at {0}", nextPasswordChangeTimestamp));
            Serializer.Serialize(this);

            HasWarned = false;
        }

        /// <summary>
        /// Recalculates the epoch times for the new/next passwords.
        /// </summary>
        private void UpdateEpochs()
        {
            PasswordChangeEpoch = Time.CalculateNewPasswordChangeEpoch(PasswordChangeFrequency, PasswordMultipler);
            PasswordPrewarnEpoch = Time.CalculatePrewarningEpoch(PasswordChangeEpoch, PasswordPrewarnTime, PasswordMultipler);
        }

        /// <summary>
        /// Generates an alphanumeric password of specified length.
        /// </summary>
        /// <returns></returns>
        private string GeneratePassword()
        {
            const int passwordLength = 10;
            var password = new char[passwordLength];
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            for (int i = 0; i < passwordLength; i++)
                password[i] = chars[R.Next(chars.Length)];

            return new string(password);
        }
    }
}
