using System;
using System.IO;
using System.Linq;
using RPIMashUnit.Helper;
using System.Threading;

namespace RPIMashUnit.Core
{
    class Ruckus
    {
        private string RuckusHost { get; set; }
        private int RuckusPort { get; set; }
        private string RuckusUser { get; set; }
        private string RuckusPass { get; set; }
        private int RuckusTimeout { get; set; }

        public Ruckus()
        {
            const string settingsConfig = "settings.cfg";

            if (!File.Exists(settingsConfig))
                throw new FileNotFoundException(string.Format("Cannot find {0}", settingsConfig));
            
            // Parse the various settings and save them to neccesary properties.
            foreach (string line in File.ReadAllLines(settingsConfig).Where(s => s.Length > 0 && s[0] != '#'))
            {
                var split = line.Split('=');
                string setting = split[0];
                string value = split[1];

                switch (setting)
                {
                    case "rkshost":
                        RuckusHost = value;
                        break;

                    case "rksport":
                        RuckusPort = Convert.ToInt32(value);
                        break;

                    case "rksuser":
                        RuckusUser = value;
                        break;

                    case "rkspass":
                        RuckusPass = value;
                        break;

                    case "rkstimeout":
                        RuckusTimeout = Convert.ToInt32(value);
                        break;
                }
            }
        }

        /// <summary>
        /// Changes the password of the Ruckus device.
        /// </summary>
        /// <param name="password">The new password for the device.</param>
        /// <returns>True if the password is changed successfully or false if otherwise.</returns>
        public bool ChangePassword(string password)
        {
            Log.DebugWriteLine("Changing Ruckus password...");

            // Have to disable as the Ruckus unit is blocking ping requests.
            /*if (Network.IsHostOnline(ruckusHost))
            {
                Log.ErrorWriteLine(String.Format("Cannot change Ruckus password, the device is offline ({0}).", ruckusHost));
            }*/

            // Only passwords between 8-63 can be used.
            if (password.Length < 8 || password.Length > 63)
            {
                Log.ErrorWriteLine("Password must be between 8-63 characters.");
                return false;
            }

            // Create a new telnet connection and login.
            var telnet = new TelnetConnection(RuckusHost, RuckusPort);
            telnet.Login(RuckusUser, RuckusPass, RuckusTimeout);

            // Check our connection attempt was successful.
            if (telnet.IsConnected)
                Log.DebugWriteLine("Ruckus login successful.");


            int[] wlanArray = { 4, 5, 6, 7, 12, 13, 14, 15 };

            foreach (int interfaceNum in wlanArray)
            {
                // While loop to successfully navigate the password change procedure.
                while (telnet.IsConnected)
                {
                    string output = telnet.Read();

                    if (output.Contains("Wireless Encryption Type:"))
                    {
                        telnet.WriteLine("");
                    }
                    else if (output.Contains("WPA Protocol Version:"))
                    {
                        telnet.WriteLine("");
                    }
                    else if (output.Contains("WPA Authentication Type:"))
                    {
                        telnet.WriteLine("");
                    }
                    else if (output.Contains("WPA Cipher Type:"))
                    {
                        telnet.WriteLine("");
                    }
                    else if (output.Contains("WPA PassPhrase:"))
                    {
                        telnet.WriteLine(password);
                        break;
                    }
                    else if (output.Contains("WPA no error"))
                    {
                        Log.DebugWriteLine(string.Format("[DEBUG] wlan{0} pass successfully changed to {1}", interfaceNum, password));

                        if (interfaceNum == 15)
                        {
                            telnet.WriteLine("quit");
                            telnet.CloseConnection();
                            return true;
                        }
                        else
                            output = "";
                    }
                    else
                    {
                        telnet.WriteLine(string.Format("set encryption wlan{0}", interfaceNum));
                    }
                }
                Thread.Sleep(1000);
            }

            return true;
        }
    }
}
