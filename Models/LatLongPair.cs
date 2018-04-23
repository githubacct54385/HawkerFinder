using System;

namespace HawkerFinder.Models {
    class LatLongPair {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public LatLongPair (double latitude, double longitude) {
            this.latitude = latitude;
            this.longitude = longitude;
        }
    }
}