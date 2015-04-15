using Raspberry.IO.GeneralPurpose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPiController
{
    internal class RaspberryController : IDisposable
    {
        OutputPinConfiguration _redLed;
        OutputPinConfiguration _greenLed;
        OutputPinConfiguration _yellowLed;
        OutputPinConfiguration[] _leds;
        GpioConnection _conn;

        public void Setup()
        {
            _redLed = ConnectorPin.P1Pin12.Output();
            _yellowLed = ConnectorPin.P1Pin16.Output();
            _greenLed = ConnectorPin.P1Pin18.Output();
            _leds = new OutputPinConfiguration[] { _redLed, _greenLed, _yellowLed };
            _conn = new GpioConnection(_leds);

            _conn[_greenLed] = true;
            _conn[_yellowLed] = true;
            _conn[_redLed] = true;
        }

        public void Set(string presence)
        {
            _conn[_redLed] = false;
            _conn[_yellowLed] = false;
            _conn[_greenLed] = false;

            switch (presence)
            {
                case "DoNotDisturb":
                case "Busy":
                    _conn[_redLed] = true;
                    break;
                case "TemporarilyAway":
                case "Away":
                    _conn[_yellowLed] = true;
                    break;
                case "Free":
                    _conn[_greenLed] = true;
                    break;
                case "FreeIdle":
                    _conn[_greenLed] = true;
                    _conn[_yellowLed] = true;
                    break;
                case "BusyIdle":                    
                    _conn[_redLed] = true;
                    _conn[_yellowLed] = true;
                    break;
                default:
                    break;
            }
        }

        public void SetStatus(QueueStatus status)
        {
            _conn[_redLed] = false;
            _conn[_yellowLed] = false;
            _conn[_greenLed] = false;

            switch (status)
            {
                case QueueStatus.Ready:
                    Blink(_greenLed, 4);
                    Blink(_yellowLed, 4);
                    Blink(_redLed, 4);
                    break;
                case QueueStatus.Error:
                    Blink(_redLed, 6);
                    System.Threading.Thread.Sleep(1000);
                    _conn[_redLed] = true;
                    break;
                case QueueStatus.WaitingForNetwork:
                    Blink(_yellowLed, 2);
                    break;
                default:
                    break;
            }
        }

        private void Blink(OutputPinConfiguration _ledPin, int count)
        {
            // GpioConnection.Blink does not work (on rpi2 at least?), so this is my own implementation
            bool enable = true;
            for (int i = 0; i < count * 2; i++)
            {
                _conn[_ledPin] = enable;
                Thread.Sleep(250);
                enable = !enable;
            }
            _conn[_ledPin] = false;
        }

        public void Dispose()
        {
            _conn.Close();
        }
    }
}
