using System;

namespace HawkerFinder.Models {
    public class Address {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addr { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}