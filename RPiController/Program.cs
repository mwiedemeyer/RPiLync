using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RPiController
{
    class Program
    {
        private static QueueController _queue;
        private static RaspberryController _rpi;

        static void Main(string[] args)
        {
            // seems to be required to work with Azure SSL on raspberry
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => { return true; };

            Logger.LogInfo("Starting...");

            _rpi = new RaspberryController();
            _rpi.Setup();

            _queue = new QueueController();
            _queue.NewMessageReceived += Queue_NewMessageReceived;
            _queue.StatusChange += Queue_StatusChange;

            Logger.LogInfo("... succeeded. Waiting for messages...");

            _queue.StartLoop();

            _rpi.Dispose();
        }

        static void Queue_StatusChange(object sender, StatusChangeEventArgs e)
        {
            Logger.LogInfo($"-> Status changed: {e.Status}");

            _rpi.SetStatus(e.Status);
        }

        private static void Queue_NewMessageReceived(object sender, NewMessageEventArgs e)
        {
            Logger.LogInfo($"-> Message received: User={e.User} Presence={e.Presence}");

            _rpi.Set(e.User, e.Presence);
        }
    }
}
