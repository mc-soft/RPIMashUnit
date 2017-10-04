using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPIMashUnit.Helper
{
    internal class ErrorHandler
    {
        public static void LogError(Exception e)
        {
            Console.WriteLine(e.ToString());
            Console.ReadLine();
        }
    }
}
