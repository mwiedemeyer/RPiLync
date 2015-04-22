using Raspberry.IO.GeneralPurpose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiController
{
    internal class PinConfigMapping
    {
        public PinConfigMapping(string user, ConnectorPin redPin, ConnectorPin yellowPin, ConnectorPin greenPin)
        {
            User = user;
            _redLedPinConfiguration = redPin.Output().Name(user + "_RED");
            _yellowLedPinConfiguration = yellowPin.Output().Name(user + "_YELLOW");
            _greenLedPinConfiguration = greenPin.Output().Name(user + "_GREEN");
        }

        public string User { get; private set; }
        public ConnectedPin RedLed { get; private set; }
        public ConnectedPin YellowLed { get; private set; }
        public ConnectedPin GreenLed { get; private set; }

        private OutputPinConfiguration _redLedPinConfiguration;
        private OutputPinConfiguration _yellowLedPinConfiguration;
        private OutputPinConfiguration _greenLedPinConfiguration;

        internal void AddPinsToConnection(GpioConnection _conn)
        {
            _conn.Add(_redLedPinConfiguration);
            _conn.Add(_yellowLedPinConfiguration);
            _conn.Add(_greenLedPinConfiguration);

            RedLed = _conn.Pins[User + "_RED"];
            YellowLed = _conn.Pins[User + "_YELLOW"];
            GreenLed = _conn.Pins[User + "_GREEN"];
        }

        internal void Set(bool red, bool yellow, bool green)
        {
            RedLed.Enabled = red;
            YellowLed.Enabled = yellow;
            GreenLed.Enabled = green;
        }
    }
}
