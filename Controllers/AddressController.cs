using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HawkerFinder.Data;
using HawkerFinder.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HawkerFinder.Controllers {
  public class AddressController : Controller {
    private readonly HawkerContext _context;

    private const string closestHawkersTemplateQuery =
      @"declare @mylat fLOAT, @mylong FLOAT
        Select @mylat = {0}, @mylong = {1}
        SELECT top 5 ABS(dbo.DictanceKM(@mylat, Address.latitude, @mylong, Address.longitude)) DistanceKm, * from Address
        ORDER BY ABS(dbo.DictanceKM(@mylat, Address.latitude, @mylong, Address.longitude)) ";

    public AddressController (HawkerContext context) {
      _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index () {
      return View (await _context.Addresses.ToListAsync ());
    }

    // returns some of the closest hawker centres to a given coordinate
    [HttpPost]
    public IActionResult CloseHawkers (double givenLatitude, double givenLongitude) {
      // run query
      Distance[] closestAddresses = GetClosestAddrs (givenLatitude, givenLongitude);
      // return results
      var dists = new List<Distance> ();
      dists.AddRange (closestAddresses);
      return View (dists);
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
              double dist = dr.GetDouble (0);
              int id = dr.GetInt32 (1);
              string address = dr.GetString (2);
              string locationName = dr.GetString (3);
              double latitude = dr.GetDouble (4);
              double longitude = dr.GetDouble (5);
              Distance distance = new Distance {
                Id = id,
                Name = locationName,
                Addr = address,
                latitude = latitude,
                longitude = longitude,
                distance = dist
              };
              closestAddresses[index++] = distance;
            }
          }
        }
      }
      return closestAddresses;
    }

    public IActionResult Error () {
      return View (new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}