using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;

namespace temp
{
    class Example
    {
        /** Create Logger */
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /** Log few lines */
        public static void Main(String[] args)
        {
            Console.Error.WriteLine("Sending warning messages, line by line..");

            log.Info("Sending warning messages, line by line..");

            for (int i = 0; i < 1000; ++i)
            {
                log.Warn("Warning message " + i);
                Thread.Sleep(1000);
            }
        }
    }
}
