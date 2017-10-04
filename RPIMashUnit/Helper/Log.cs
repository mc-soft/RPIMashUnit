using System;

namespace RPIMashUnit.Helper
{
    internal class Log
    {
        public static bool IsDebug = true; // TODO: Change to false.

        /// <summary>
        /// Outputs a message with a [INFO] prefix.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public static void WriteLine(string message)
        {
            Console.WriteLine("{0} -  [INFO] {1}", Time.EpochToTimestamp(Time.GetCurrentEpoch()), message);
        }

        /// <summary>
        /// Outputs a message with a DarkYellow [DEBUG] prefix.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public static void DebugWriteLine(string message)
        {
            if (!IsDebug) return;

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("{0} - [DEBUG] ", Time.EpochToTimestamp(Time.GetCurrentEpoch()));
            Console.ResetColor();

            Console.WriteLine(message);
        }

        /// <summary>
        /// Outputs a message with a DarkRed [ERROR] prefix.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public static void ErrorWriteLine(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("{0} - [ERROR] ", Time.EpochToTimestamp(Time.GetCurrentEpoch()));
            Console.ResetColor();

            Console.WriteLine(message);
        }

        /// <summary>
        /// Display simple ASCII text header along with version/credits.
        /// </summary>
        public static void DisplayHeader()
        {
            Console.WriteLine(@",------. ,------. ,--.,--.   ,--.               ,--.     ");
            Console.WriteLine(@"|  .--. '|  .--. '|  ||   `.'   | ,--,--. ,---. |  ,---.  v1.2.2");
            Console.WriteLine(@"|  '--'.'|  '--' ||  ||  |'.'|  |' ,-.  |(  .-' |  .-.  |");
            Console.WriteLine(@"|  |\  \ |  | --' |  ||  |   |  |\ '-'  |.-'  `)|  | |  | Created by:");
            Console.WriteLine("`--' '--'`--'     `--'`--'   `--' `--`--'`----' `--' `--' Matthew Croston\n");
        }

        /*      Change log:
         *      
         *      v1.0.0 - v1.0.1:
         *          Changed Serializer class to only Serialize/Deserialize class of the Settings type.
         * 
         *      v1.0.1 - v1.0.2:
         *          Removed nested classes from the Settings class as these were failing to Deserialize properly.
         *          
         *      v1.0.2 - v1.0.3:
         *          The try-catch that detects failure initializing the devices/settings class was looping 4 times
         *          instead of 3, this has been corrected.
         *          
         *      v1.0.3 - v1.1.0:
         *          First major(ish) update, complete code rewrite/clean and code fully commented.
         *          
         *      v1.1.0 - v1.1.1:
         *          Reset the ReportStatusEpoch to 0 upon boot so the initial boot status email is sent correctly.
         *          
         *      v1.1.1 - v1.1.2:
         *          Called Devices.UpdateDeviceStatus() if an exception is thrown in the main loop, on some occasions
         *          the external arp-scan software was failing to find the Ruckus unit - this should correct that.
         *          
         *      v1.1.2 - v1.2.0:
         *          Due to a new network configuration, the devices can now be assigned static IPs. I have removed
         *          the functionality to find the IP using a third-party ARP scanning tool for more stability.
         *          
         *      v1.2.0 - v1.2.1:
         *          Added a check to make sure the network is online before continuing the main loop.
         *          
         *      v1.2.1 - v1.2.2:
         *          If the password is already set, send it in the status/boot email.
         *          
         */
    }
}
