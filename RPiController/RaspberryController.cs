using Raspberry.IO.GeneralPurpose;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPiController
{
    internal class RaspberryController : IDisposable
    {
        PinConfigMappings _mapping = new PinConfigMappings();
        GpioConnection _conn;

        public void Setup()
        {
            _conn = new GpioConnection();

            var configKeys = ConfigurationManager.AppSettings.AllKeys.Where(p => p.StartsWith("UserPortMapping"));
            foreach (var key in configKeys)
            {
                var user = key.Split(':')[1];
                var ports = ConfigurationManager.AppSettings[key].Split(',');
                var redPin = (ConnectorPin)Enum.Parse(typeof(ConnectorPin), ports[0]);
                var yellowPin = (ConnectorPin)Enum.Parse(typeof(ConnectorPin), ports[1]);
                var greenPin = (ConnectorPin)Enum.Parse(typeof(ConnectorPin), ports[2]);
                var map = new PinConfigMapping(user, redPin, yellowPin, greenPin);
                map.AddPinsToConnection(_conn);
                _mapping.Add(map);
            }

            Blink(true, true, true, 5);
        }

        public void Set(string user, string presence)
        {
            var mapping = _mapping.GetByUser(user);

            mapping.Set(false, false, false);

            switch (presence)
            {
                case "DoNotDisturb":
                case "Busy":
                    mapping.Set(true, false, false);
                    break;
                case "TemporarilyAway":
                case "Away":
                    mapping.Set(false, true, false);
                    break;
                case "Free":
                    mapping.Set(false, false, true);
                    break;
                case "FreeIdle":
                    mapping.Set(false, true, true);
                    break;
                case "BusyIdle":
                    mapping.Set(true, true, false);
                    break;
                default:
                    break;
            }
        }

        public void SetStatus(QueueStatus status)
        {
            _mapping.SetAll(false, false, false);

            switch (status)
            {
                case QueueStatus.Ready:
                    Blink(true, true, true, 4);
                    break;
                case QueueStatus.Error:
                    Blink(true, false, false, 6);
                    break;
                case QueueStatus.WaitingForNetwork:
                    Blink(false, true, false, 3);
                    break;
                default:
                    break;
            }
        }

        private void Blink(bool red, bool yellow, bool green, int count)
        {
            var isRed = red;
            var isYellow = yellow;
            var isGreen = green;

            // GpioConnection.Blink does not work (on rpi2 at least?), so this is my own implementation            
            for (int i = 0; i < count * 2; i++)
            {
                _mapping.SetAll(red, yellow, green);
                Thread.Sleep(250);
                if (isRed) red = !red;
                if (isYellow) yellow = !yellow;
                if (isGreen) green = !green;
            }

            _mapping.SetAll(false, false, false);
        }

        public void Dispose()
        {
            _conn.Close();
        }
    }
}
