using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GoogleMapsApi;
using GoogleMapsApi.Entities.Geocoding.Request;
using GoogleMapsApi.Entities.Geocoding.Response;
using HawkerFinder.Data;
using HawkerFinder.Geocoding;
using HawkerFinder.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HawkerFinder.Controllers {
  public class AddressController : Controller {
    private readonly HawkerContext _context;
    private const string googleMapsAPIKey = "AIzaSyC6v5-2uaq_wusHDktM9ILcqIrlPtnZgEk";

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

    // returns five of the closest hawker centres to a given coordinate
    [HttpPost]
    public IActionResult CloseHawkersAsync (string address) {
      List<HawkerDistance> distances = new List<HawkerDistance> ();
      string mapsMarkers = "";
      try {
        GeocodingResponse coordinates = GeocodeAddress (address);
        CheckForCoordinatesErrors (coordinates);
        distances = FiveClosestDistancesToCoordinates (coordinates);
        mapsMarkers = CreateStringMarkers (distances);
        bool noMarkersAvailable = String.IsNullOrEmpty (mapsMarkers);
        if (noMarkersAvailable) {
          return RedirectToAction ("Index");
        }
        ViewBag.Markers = mapsMarkers;
        ViewBag.SearchAddress = address;
        return View ("closehawkers");
      } catch (System.Exception ex) {
        Console.WriteLine ("An exception occurred in AddressController/CloseHawkers");
        Console.WriteLine ($"Exception text: {ex.Message}");
        ViewBag.Exception = ex.Message;
        return View ("closehawkers");
      }
    }

    [HttpPost]
    public IActionResult ReverseGeocode (string rvgLat, string rvgLng) {
      try {
        ReverseGeocodingRequest request = new ReverseGeocodingRequest (rvgLat, rvgLng);
        ReverseGeocodingResponse response = new ReverseGeocodingResponse (request, googleMapsAPIKey);
        bool responseIsOK = response.responseStatus == HttpStatusCode.OK;
        if (responseIsOK) {
          ViewBag.Lat = rvgLat;
          ViewBag.Lng = rvgLng;
          return View (response.Address);
        }
        ViewBag.BadResponseMessage = response.responseStatus;
        return View ();
      } catch (System.Exception) {
        return View ();
      }

    }

    private static void CheckForCoordinatesErrors (GeocodingResponse coordinates) {
      bool incorrectStatus = coordinates.Status != Status.OK;
      if (incorrectStatus) {
        throw new Exception ($"Cannot proceed - Geocoding Response gave invalid status: {coordinates.Status}");
      }
      bool wrongNumberOfCoordinates = coordinates.Results.Count () != 1;
      if (wrongNumberOfCoordinates) {
        throw new Exception ($"Cannot proceed - Geocoding Response gave incorrect number of coordinates.  Count: {coordinates.Results.Count()}");
      }
    }

    private static GeocodingResponse GeocodeAddress (string address) {
      var geocodingRequest = new GeocodingRequest {
        ApiKey = googleMapsAPIKey,
        Address = address
      };
      return GoogleMaps.Geocode.Query (geocodingRequest);
    }

    private string CreateStringMarkers (List<HawkerDistance> dists) {
      try {
        if (!dists.Any ()) {
          throw new Exception ("No nearby Hawkers.  Unable to proceed");
        }
        string markers = "[";
        foreach (var distance in dists) {
          markers += "{";
          markers += $"'title':'{distance.Name}',";
          markers += $"'lat':'{distance.latitude}',";
          markers += $"'lng':'{distance.longitude}',";
          markers += $"'description':'{distance.Name}'";
          markers += "},";
        }
        markers += "];";
        return markers;
      } catch (System.Exception ex) {
        throw new Exception ($"An exception occurred in CreateStringMarkers().  The exception text says: {ex.Message}");
      }
    }

    private List<HawkerDistance> FiveClosestDistancesToCoordinates (GeocodingResponse coordinates) {
      try {
        HawkerDistance[] closestAddresses = FindClosestAddresses (coordinates);
        var distances = new List<HawkerDistance> ();
        distances.AddRange (closestAddresses);
        return distances;
      } catch (System.Exception ex) {
        throw new Exception ($"An exception occurred in FiveClosestDistancesToCoordinates.  The exception text says: {ex.Message} ");
      }
    }

    // Finds the closest Addresses by calling a server-side Stored Procedure
    private HawkerDistance[] FindClosestAddresses (GeocodingResponse coordinates) {
      HawkerDistance[] fiveClosestAddresses = new HawkerDistance[5];
      int arrayIndex = 0;

      using (SqlConnection conn = new SqlConnection (_context.Database.GetDbConnection ().ConnectionString)) {
        conn.Open ();

        SqlCommand cmd = new SqlCommand ("dbo.CLOSEST_HAWKERS", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        double lat = coordinates.Results.First ().Geometry.Location.Latitude;
        double lng = coordinates.Results.First ().Geometry.Location.Longitude;
        AddSQLParameter (cmd, "@Lat", lat);
        AddSQLParameter (cmd, "@Lng", lng);

        // execute the command
        using (SqlDataReader dataReader = cmd.ExecuteReader ()) {
          // iterate through results, printing each to console
          while (CanReadMore (arrayIndex, dataReader)) {
            HawkerDistance distance = CreateDistance (dataReader.GetDouble (0),
              dataReader.GetInt32 (1), dataReader.GetString (2), dataReader.GetString (3),
              dataReader.GetDouble (4), dataReader.GetDouble (5));
            fiveClosestAddresses[arrayIndex++] = distance;
          }
          return fiveClosestAddresses;
        }
      }
    }

    private static void AddSQLParameter (SqlCommand cmd, string paramName, double paramValue) {
      cmd.Parameters.Add (new SqlParameter (paramName, SqlDbType.Float));
      cmd.Parameters[paramName].Value = paramValue;
    }

    private static bool CanReadMore (int arrayIndex, SqlDataReader dataReader) {
      return dataReader.Read () && arrayIndex < 5;
    }

    private static HawkerDistance CreateDistance (double dist, int id,
      string address, string locationName, double latitude, double longitude) {
      return new HawkerDistance {
        Id = id, Name = locationName, Addr = address,
          latitude = latitude, longitude = longitude, distance = dist
      };
    }

    public IActionResult Error () {
      return View (new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}