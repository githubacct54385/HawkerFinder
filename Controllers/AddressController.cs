using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using HawkerFinder.Data;
using HawkerFinder.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HawkerFinder.Controllers {
  public class AddressController : Controller {
    private readonly HawkerContext _context;

    private const string closestHawkersTemplateQuery =
      @"declare @mylat fLOAT, @mylong FLOAT
        Select @mylat = {0}, @mylong = {1}
        SELECT top 5 ABS(dbo.DictanceKM(@mylat, Address.latitude, @mylong, 
        Address.longitude)) DistanceKm, * from Address
        ORDER BY ABS(dbo.DictanceKM(@mylat, Address.latitude, @mylong, 
        Address.longitude)) ";

    public AddressController (HawkerContext context) {
      _context = context;
    }

    [HttpGet]
    public IActionResult Index () {
      return View ();
    }

    [HttpGet]
    public async Task<IActionResult> SeeAll () {
      return View (await _context.Addresses.ToListAsync ());
    }

    // returns some of the closest hawker centres to a given coordinate
    [HttpPost]
    public async Task<IActionResult> CloseHawkersAsync (string address) {
      try {
        // query Google Geocoding service on the server
        LatLongPair pair = await GeocodeAddressAsync (address);
        // use the coordinates to get the five closest hawker centres
        List<Distance> dists = DistancesFromCoordinates (pair);
        return View ("closehawkers", dists);
      } catch (System.Exception ex) {
        Console.WriteLine ("An exception occurred in AddressConteoller/CloseHawkers");
        Console.WriteLine ($"Exception text: {ex.Message}");
        return View ("closehawkers");
      }
    }

    private async Task<LatLongPair> GeocodeAddressAsync (string address) {
      string url = GoogleMapsAPIQuery (address);
      using (var client = new HttpClient ()) {
        var response = await client.GetAsync (url);
        if (response.StatusCode == HttpStatusCode.OK) {
          LatLongPair pair = ParseJSONResponse (response);
          return pair;
        } else {
          Exception ex = new Exception ("HTTP call to Google Maps API returned non-OK status");
          throw ex;
        }
      }
    }

    private static LatLongPair ParseJSONResponse (HttpResponseMessage response) {
      var content = response.Content.ReadAsStringAsync ();
      JObject parsedJson = JObject.Parse (content.Result);
      JToken firstResults = parsedJson["results"][0];
      JToken loc = firstResults["geometry"]["location"];
      JToken lat = loc["lat"];
      JToken lng = loc["lng"];
      LatLongPair pair = new LatLongPair (Double.Parse (lat.ToString ()),
        Double.Parse (lng.ToString ()));
      return pair;
    }

    private string GoogleMapsAPIQuery (string address) {
      UriBuilder builder = new UriBuilder ("https://maps.googleapis.com/maps/api/geocode/json");
      builder.Port = -1;
      var query = HttpUtility.ParseQueryString (builder.Query);
      query["address"] = address;
      query["key"] = _context.GoogleAPIKey;
      builder.Query = query.ToString ();
      string url = builder.ToString ();
      return url;
    }

    private List<Distance> DistancesFromCoordinates (LatLongPair coordinates) {
      // run query
      Distance[] closestAddresses = GetClosestAddrs (coordinates.latitude, coordinates.longitude);
      // return results
      var dists = new List<Distance> ();
      dists.AddRange (closestAddresses);
      return dists;
    }

    private Distance[] GetClosestAddrs (double givenLatitude, double givenLongitude) {
      // only return up to five addresses
      Distance[] closestAddresses = new Distance[5];
      int index = 0;
      using (SqlConnection conn =
        new SqlConnection (_context.Database.GetDbConnection ().ConnectionString)) {
        conn.Open ();
        string closestHawkersQuery =
          string.Format (closestHawkersTemplateQuery, givenLatitude, givenLongitude);
        using (SqlCommand cmd = new SqlCommand (closestHawkersQuery, conn)) {
          using (SqlDataReader dr = cmd.ExecuteReader ()) {
            while (dr.Read () && index < 5) {
              Distance distance = CreateDistance (dr.GetDouble (0),
                dr.GetInt32 (1), dr.GetString (2), dr.GetString (3),
                dr.GetDouble (4), dr.GetDouble (5));
              closestAddresses[index++] = distance;
            }
          }
        }
      }
      return closestAddresses;
    }

    private static Distance CreateDistance (double dist, int id,
      string address, string locationName, double latitude, double longitude) {
      return new Distance { Id = id, Name = locationName, Addr = address,
        latitude = latitude, longitude = longitude, distance = dist};
    }

    public IActionResult Error () {
      return View (new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}