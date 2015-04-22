using System.Collections.Generic;
using System.Linq;

namespace RPiController
{
    internal class PinConfigMappings
    {
        private List<PinConfigMapping> _mappings;

        public PinConfigMappings()
        {
            _mappings = new List<PinConfigMapping>();
        }

        public void Add(PinConfigMapping mapping)
        {
            _mappings.Add(mapping);
        }

        public void SetAll(bool red, bool yellow, bool green)
        {
            foreach (var map in _mappings)
            {
                map.Set(red, yellow, green);
            }
        }

        public PinConfigMapping GetByUser(string user)
        {
            return _mappings.FirstOrDefault(p => p.User == user);
        }
    }
}