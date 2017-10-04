using System;
using System.IO;
using System.Text;

namespace RPIMashUnit.Helper
{
    internal class ErrorHandler
    {
        private const string ErrorLogEmail = "mash@lde.co.uk";

        public static void LogError(Exception ex)
        {
            Log.DebugWriteLine("Logging exception...");

            // Creates an error log that will be used for debugging purposes.
            StringBuilder errorLog = new StringBuilder();
            errorLog.AppendLine(string.Format("An exception occured at {0}\n",
                Time.EpochToTimestamp(Time.GetCurrentEpoch())));
            errorLog.AppendLine(string.Format("{0}\n", ex));
            errorLog.AppendLine(string.Format("Target Site:\n{0}\n", ex.TargetSite));
            errorLog.AppendLine(string.Format("Source:\n{0}\n", ex.Source));
            errorLog.AppendLine(string.Format("Inner Exception:\n{0}", ex.InnerException));


            if (Network.IsOnline())
            {
                Log.DebugWriteLine("Internet access available, sending error log via email.");

                // Network is online so send an email containing error information.
                var e = new SendMail(ErrorLogEmail);
                e.SendExceptionDetails(errorLog);
            }
            else
            {
                string filename = Time.EpochToTimestamp(Time.GetCurrentEpoch());

                Log.DebugWriteLine(string.Format("We appear to be offline, saving error log to {0}.", filename));

                // Network is offline so save the error log to file.
                if (!Directory.Exists("Errors"))
                    Directory.CreateDirectory("Errors");

                File.WriteAllText(string.Format("Errors/{0}.log", filename), errorLog.ToString());
            }
        }
    }
}
