using Microsoft.Lync.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LyncAdapter
{
    // This is lyncx from Jon Gallant. Original can be found here: https://github.com/jonbgallant/beakn/tree/master/desktop/lyncx
    public class LyncManager
    {
        protected LyncClient _lyncClient = null;

        public event EventHandler<AvailabilityEventArgs> AvailabilityChanged;

        protected virtual void OnAvailabilityChanged(AvailabilityEventArgs e)
        {
            EventHandler<AvailabilityEventArgs> handler = AvailabilityChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public LyncManager() { }

        public void Setup()
        {
            while (_lyncClient == null)
            {
                try
                {
                    _lyncClient = LyncClient.GetClient();
                    _lyncClient.StateChanged -= new EventHandler<ClientStateChangedEventArgs>(Client_StateChanged);
                    _lyncClient.StateChanged += new EventHandler<ClientStateChangedEventArgs>(Client_StateChanged);

                }
                catch (ClientNotFoundException)
                {
                    // Eat this for now.  It just means that the Lync client isn't running on the desktop.  
                    // TODO figure out a better way to do this.
                    Thread.Sleep(5000);
                }
            }

            if (_lyncClient.Self != null && _lyncClient.Self.Contact != null)
            {
                _lyncClient.Self.Contact.ContactInformationChanged -= new EventHandler<ContactInformationChangedEventArgs>(SelfContact_ContactInformationChanged);
                _lyncClient.Self.Contact.ContactInformationChanged += new EventHandler<ContactInformationChangedEventArgs>(SelfContact_ContactInformationChanged);

                SetAvailability();
            }
        }

        public Availability Availability
        {
            get
            {
                if (_lyncClient != null && _lyncClient.State == ClientState.SignedIn)
                {
                    //Get the current availability value from Lync
                    ContactAvailability contactAvailability = 0;

                    contactAvailability = (ContactAvailability)_lyncClient.Self.Contact.GetContactInformation(ContactInformationType.Availability);
                    string availabilityName = Enum.GetName(typeof(ContactAvailability), contactAvailability);

                    return new Availability(contactAvailability, availabilityName);
                }
                return null;
            }
        }

        private void SelfContact_ContactInformationChanged(object sender, ContactInformationChangedEventArgs e)
        {
            //Only update the contact information in the user interface if the client is signed in.
            //Ignore other states including transitions (e.g. signing in or out).
            if (_lyncClient.State == ClientState.SignedIn)
            {
                //Get from Lync only the contact information that changed.
                if (e.ChangedContactInformation.Contains(ContactInformationType.Availability))
                {
                    //Use the current dispatcher to update the contact's availability in the user interface.
                    SetAvailability();
                }
            }
        }

        private void SetAvailability()
        {
            var availability = Availability;
            if (availability != null)
            {
                OnAvailabilityChanged(new AvailabilityEventArgs(availability));
            }
        }

        private void Client_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case ClientState.SignedIn:
                    Setup();
                    break;
                case ClientState.SigningOut:
                    OnAvailabilityChanged(new AvailabilityEventArgs(new Availability(ContactAvailability.Offline, "Offline")));
                    Setup();
                    break;
                default:
                    break;
            }
        }
    }

    public class AvailabilityEventArgs : EventArgs
    {
        public AvailabilityEventArgs(Availability availability)
        {
            Availability = availability;
        }

        public Availability Availability { get; set; }
    }

    public class Availability
    {
        public Availability(ContactAvailability contactAvailability, string availabilityName)
        {
            ContactAvailability = contactAvailability;
            AvailabilityName = availabilityName;
        }

        public ContactAvailability ContactAvailability { get; set; }
        public string AvailabilityName { get; set; }
    }
}
