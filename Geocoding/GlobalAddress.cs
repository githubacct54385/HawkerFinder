using System;

namespace HawkerFinder.Geocoding {
    public class GlobalAddress
    {
        public string streetAddress { get; internal set; }
        public double lat { get; internal set; }
        public double lng { get; internal set; }
        public string country { get; internal set; }
    }
}