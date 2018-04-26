using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace HawkerFinder.Geocoding {
    class ReverseGeocodingResponse {
        private const string baseUrl = "https://maps.googleapis.com/maps/api/geocode/json";
        internal HttpStatusCode responseStatus;
        internal GlobalAddress Address;
        public ReverseGeocodingResponse (ReverseGeocodingRequest request, string googleMapsAPIKey) {
            string query = ReverseGeocodingQuery (request, googleMapsAPIKey);
            var result = RunQueryAsync (query);
            Address = result.Result;
        }

        private async Task<GlobalAddress> RunQueryAsync (string query) {
            try {
                using (var client = new HttpClient ()) {
                    var response = await client.GetAsync (query);
                    this.responseStatus = response.StatusCode;
                    bool responseIsNotOK = response.StatusCode != HttpStatusCode.OK;
                    if (responseIsNotOK) {
                        return new GlobalAddress();
                    }
                    return ParsedJSONAddress (response);
                }
            } catch (System.Exception ex) {
                Console.WriteLine ($"Exception occurred in RunQueryAsync.  Here is the message: {ex.Message}");
                return new GlobalAddress ();
            }
        }

        private static GlobalAddress ParsedJSONAddress (HttpResponseMessage response) {
            GlobalAddress globalAddress = new GlobalAddress ();
            var content = response.Content.ReadAsStringAsync ();
            JObject parsedJson = JObject.Parse (content.Result);
            JToken firstResults = parsedJson["results"][0];
            JToken addressComponents = firstResults["address_components"];
            foreach (var component in addressComponents) {
                foreach (var type in component["types"]) {
                    if (IsNotCountryType (type)) continue;
                    JToken countryToken = component["long_name"];
                    globalAddress.country = countryToken.ToString ();
                    break;
                }
            }
            JToken streetAddress = firstResults["formatted_address"];
            globalAddress.streetAddress = streetAddress.ToString ();
            JToken loc = firstResults["geometry"]["location"];
            JToken lat = loc["lat"];
            JToken lng = loc["lng"];
            globalAddress.lat = Double.Parse (lat.ToString ());
            globalAddress.lng = Double.Parse (lng.ToString ());
            return globalAddress;
        }

        private static bool IsNotCountryType (JToken type) {
            return type.ToString () != "country";
        }

        private string ReverseGeocodingQuery (ReverseGeocodingRequest request, string googleMapsAPIKey) {
            UriBuilder builder = new UriBuilder (baseUrl);
            builder.Port = -1;
            string coordinatesQuery = $"latlng={request.lat},{request.lng}";
            string apiKeyQuery = $"&key={googleMapsAPIKey}";
            builder.Query += coordinatesQuery;
            builder.Query += apiKeyQuery;
            return builder.ToString ();
        }
    }
}