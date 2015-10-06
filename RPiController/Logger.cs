using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiController
{
    public static class Logger
    {
        private static object logLockObject = new object();

        public static void LogInfo(string message)
        {
            Console.WriteLine(message);
            Log("[INFO] " + message);
        }

        public static void LogError(string message)
        {
            Console.WriteLine(message);
            Log("[ERROR] " + message);
        }

        private static void Log(string message)
        {
            lock (logLockObject)
            {
                var path = "Log.txt";
                File.AppendAllText(path, string.Format("[{0}] {1}{2}", DateTime.Now, message, Environment.NewLine));
            }
        }
    }

}
