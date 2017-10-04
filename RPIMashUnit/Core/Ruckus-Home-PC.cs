using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPIMashUnit.Helper;

namespace RPIMashUnit.Core
{
    class Ruckus
    {
        private int RuckusPort { get; set; }
        private string RuckusUser { get; set; }
        private string RuckusPass { get; set; }
        private int RuckusTimeout { get; set; }

        public Ruckus()
        {
            const string settingsConfig = "settings.cfg";

            if (!File.Exists(settingsConfig))
                throw new FileNotFoundException(string.Format("Cannot find {0}", settingsConfig));
            
            foreach (string line in File.ReadAllLines(settingsConfig).Where(s => s.Length > 0 && s[0] != '#'))
            {
                var split = line.Split('=');
                string setting = split[0];
                string value = split[1];

                switch (setting)
                {
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

        public bool ChangePassword(string password, string ruckusHost)
        {
            return true; // TODO: Remove.
            

            if (Network.IsHostOnline(ruckusHost))
            {
                Log.ErrorWriteLine(String.Format("Cannot change Ruckus password, the device is offline ({0}).", ruckusHost));
            }

            Log.DebugWriteLine("Changing Ruckus password...");

            if (password.Length < 8 || password.Length > 63)
            {
                Log.ErrorWriteLine("Password must be between 8-63 characters.");
                return false;
            }

            var telnet = new TelnetConnection(ruckusHost, RuckusPort);

            telnet.Login(RuckusUser, RuckusPass, RuckusTimeout);

            if (telnet.IsConnected)
                Log.DebugWriteLine("Ruckus login successful.");

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
                    Log.DebugWriteLine(string.Format("[DEBUG] Password successfully changed to {0}", password));
                    return true;
                }
                else
                {
                    telnet.WriteLine("set encryption wlan0");
                }
            }

            return true;
        }
    }
}
