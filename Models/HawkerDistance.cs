using System;

namespace HawkerFinder.Models {
    public class HawkerDistance {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addr { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double distance {get; set; }
    }
}