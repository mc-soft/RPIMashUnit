using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RPIMashUnit.Helper;

namespace RPIMashUnit.Core
{
    internal class Devices
    {
        public List<Device> DeviceList;

        internal class Device
        {
            public string IP;
            public string MAC;
            public string Name;
            public bool IsOnline;
        }

        public Devices()
        {
            UpdateDeviceStatus();
        }

        /// <summary>
        /// Updates the DeviceList with the current device status'.
        /// </summary>
        public void UpdateDeviceStatus()
        {
            Log.DebugWriteLine("Updating device status list.");

            DeviceList = GetDevices();
        }

        /// <summary>
        /// Returns the IP address of the specified MAC address.
        /// </summary>
        /// <param name="macAddress"></param>
        /// <returns>The IP address of the device MAC address specified.</returns>
        public string GetIpFromMac(string macAddress)
        {
            foreach (var device in DeviceList.Where(d => d.MAC.ToLower() == macAddress.ToLower()))
            {
                return device.IP;
            }

            throw new Exception(string.Format("Could not find a device with the MAC address {0}", macAddress));
        }

        /// <summary>
        /// Gets the device information listed in the device file, combines it with the arp information to obtain IP/online status.
        /// </summary>
        /// <returns>List of devices containing Name, IP, MAC and online status.</returns>
        private List<Device> GetDevices()
        {
            const string arpinfoFile = "arpinfo.dat";
            const string devicesConfig = "devices.cfg";

            if (!File.Exists(devicesConfig))
                throw new FileNotFoundException(string.Format("Cannot find {0}", devicesConfig));

            var devices = new List<Device>();

            foreach (string deviceInfo in File.ReadAllLines(devicesConfig).Where(s => s.Length > 0 && s[0] != '#'))
            {
                var info = deviceInfo.Split(',');

                Device d = new Device
                {
                    MAC = info[0],
                    Name = info[1]
                };

                // Uses the pre-known MAC address to lookup the IP from the arpinfo file and checks the online status of the current device.
                foreach (string line in File.ReadAllLines(arpinfoFile).Where(s => s.ToLower().Contains(d.MAC.ToLower())))
                {
                    d.IP = line.Split('\t')[0];
                    d.IsOnline = Network.IsHostOnline(d.IP);
                }

                devices.Add(d);
            }

            return devices;
        }
    }
}
