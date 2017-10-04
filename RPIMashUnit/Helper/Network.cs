using System.Net.NetworkInformation;

namespace RPIMashUnit.Helper
{
    internal class Network
    {
        /// <summary>
        /// Checks whether the device is currently connected to the internet by pinging the Google DNS (8.8.8.8)
        /// </summary>
        /// <returns>True = Online, False = Offline.</returns>
        public static bool IsOnline()
        {
            return IsHostOnline("8.8.8.8");
        }

        /// <summary>
        /// Checks whether the specified device can currently be reached via ping.
        /// </summary>
        /// <param name="ipAddress">The IP address of the device to ping.</param>
        /// <returns>True if the ping was successful, false if otherwise.</returns>
        public static bool IsHostOnline(string ipAddress)
        {
            try
            {
                using (Ping p = new Ping())
                {
                    var pingReply = p.Send(ipAddress);

                    return pingReply != null && pingReply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
