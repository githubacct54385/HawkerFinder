using System;

namespace HawkerFinder.Geocoding {
    class ReverseGeocodingRequest
    {
        public string lat { get; private set; }
        public string lng { get; private set; }

        public ReverseGeocodingRequest(string lat, string lng)
        {
            this.lat = lat;
            this.lng = lng;
        }
    }
}