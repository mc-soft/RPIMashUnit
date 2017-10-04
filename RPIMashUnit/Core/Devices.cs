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
            public string Name;
            public bool IsOnline;
        }

        public Devices()
        {
            UpdateDeviceStatus();
        }

        /// <summary>
        /// Updates the DeviceList with the current status of the devices.
        /// </summary>
        public void UpdateDeviceStatus()
        {
            Log.DebugWriteLine("Updating device status list.");

            DeviceList = GetDevices();
        }

        /// <summary>
        /// Parses the devices contained in the devices.cfg file and gather various information related to them.
        /// </summary>
        /// <returns>A list of devices containing their Name, IP and online status.</returns>
        private List<Device> GetDevices()
        {
            const string devicesConfig = "devices.cfg";

            if (!File.Exists(devicesConfig))
                throw new FileNotFoundException(string.Format("Cannot find {0}", devicesConfig));

            var devices = new List<Device>();

            foreach (string deviceInfo in File.ReadAllLines(devicesConfig).Where(s => s.Length > 0 && s[0] != '#'))
            {
                var info = deviceInfo.Split(',');

                Device d = new Device
                {
                    IP = info[0],
                    Name = info[1],
                    IsOnline = Network.IsHostOnline(info[0])
                };

                devices.Add(d);
            }

            return devices;
        }
    }
}
