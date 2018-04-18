using System;
using System.IO;
using System.Linq;
using HawkerFinder.Models;

namespace HawkerFinder.Data {
  public static class DbInitializer {
    public static void Initialize (HawkerContext context, string hawkerCentreDir) {
      context.Database.EnsureCreated ();

      // Look for any Addresss.
      if (context.Addresses.Any ()) {
        return; // DB has been seeded
      }

      // if not seeded, read from the HawkerCentres.csv file for all the addresses
      // and put them into the database
      Address[] addresses = ReadFieldsFromCSV (hawkerCentreDir);

      foreach (Address a in addresses) {
        context.Addresses.Add (a);
      }
      context.SaveChanges ();
    }

    private static int TotalLines (string filePath) {
      using (StreamReader r = new StreamReader (filePath)) {
        int i = 0;
        while (r.ReadLine () != null) { i++; }
        return i;
      }
    }

    private static Address[] ReadFieldsFromCSV (string csvPath) {
      int totalLines = TotalLines (csvPath) - 1;
      Address[] addresses = new Address[totalLines];
      using (StreamReader sr = new StreamReader (csvPath)) {
        // does row 0 have a header row?
        String line = sr.ReadLine ();
        String[] words = line.Split (',');
        if (words[1] != "Name") {
          throw new Exception ("Missing Header row in csv file.");
        }
        int iterator = 0;
        while (!sr.EndOfStream) {
          line = sr.ReadLine ();
          words = line.Split (',');
          try {
            // get the lat and long vals
            double latitude = words[3] == "" ? 0.0 : Double.Parse(words[3]);
            double longitude = words[4] == "" ? 0.0 : Double.Parse(words[4]);
            // create the address
            Address newAddress = new Address{Name=words[1], Addr=words[2], latitude=latitude, longitude=longitude};
            addresses[iterator++] = newAddress;
          } catch (System.IndexOutOfRangeException e) {
            Console.WriteLine (e.ToString ());
          } catch (System.FormatException e) {
            Console.WriteLine (e.ToString ());
          }
        }
      }
      // check if number of Addresses equals TotalLines
      if (totalLines != addresses.Length) {
        throw new Exception ("You did not read the right number of lines from the csv file");
      }
      return addresses;
    }
  }
}