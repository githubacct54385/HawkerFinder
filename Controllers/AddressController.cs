using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HawkerFinder.Data;
using HawkerFinder.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HawkerFinder.Controllers {
    public class AddressController : Controller {
        private readonly HawkerContext _context;
        public AddressController (HawkerContext context) {
            _context = context;
        }
        public async Task<IActionResult> Index () {
            return View (await _context.Addresses.ToListAsync ());
        }

        public IActionResult Error () {
            return View (new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}