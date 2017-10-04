using System;

namespace RPIMashUnit.Helper
{
    internal class Time
    {
        /// <summary>
        /// Gets the number of seconds passed since 01/01/1970.
        /// </summary>
        /// <returns>Current Epoch based on the device system time (not UTC)</returns>
        public static int GetCurrentEpoch()
        {
            TimeSpan t = DateTime.Now - new DateTime(1970, 1, 1);
            return (int)t.TotalSeconds;
        }

        /// <summary>
        /// Converts a epoch time into a readable timestamp. (dd/MM/yyyy - HH:mm)
        /// </summary>
        /// <param name="seconds">The time in seconds since 01/01/1970.</param>
        /// <returns>A converted timestamp in the format (dd/MM/yyyy - HH:mm).</returns>
        public static string EpochToTimestamp(int seconds)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            epoch = epoch.AddSeconds(seconds);

            return epoch.ToString("dd/MM/yyyy - HH:mm");
        }
        
        /// <summary>
        /// Calculates a new epoch for when the next password change should occur.
        /// </summary>
        /// <param name="frequency">The password change frequency.</param>
        /// <param name="multipler">The timing multiplier.</param>
        /// <returns>The new password change epoch.</returns>
        public static int CalculateNewPasswordChangeEpoch(int frequency, int multipler)
        {
            return GetCurrentEpoch() + (frequency * multipler);
        }

        /// <summary>
        /// Calculates a new epoch for when the next password change WARNING should occur.
        /// </summary>
        /// <param name="nextChangeEpoch">The epoch for when the next password change is occuring.</param>
        /// <param name="frequency">The password prewarning warning frequency.</param>
        /// <param name="multiplier">The timing multiplier.</param>
        /// <returns>The new password prewarning epoch.</returns>
        public static int CalculatePrewarningEpoch(int nextChangeEpoch, int frequency, int multiplier)
        {
            return nextChangeEpoch - (frequency * multiplier);
        }

        /// <summary>
        /// Calculates a new epoch for when the next report status email should be sent.
        /// </summary>
        /// <param name="frequency">The report status frequency.</param>
        /// <param name="multiplier">The timing multiplier.</param>
        /// <returns>The new report status epoch.</returns>
        public static int CalculateReportStatusEpoch(int frequency, int multiplier)
        {
            return GetCurrentEpoch() + (frequency * multiplier);
        }
    }
}
