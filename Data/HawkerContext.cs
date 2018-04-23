using Microsoft.EntityFrameworkCore;
using HawkerFinder.Models;

namespace HawkerFinder.Data {
    public class HawkerContext : DbContext {
        public HawkerContext (DbContextOptions<HawkerContext> options) : base (options) {
         }

        public DbSet<Address> Addresses { get; set; }
        public string GoogleAPIKey { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Address>().ToTable("Address");
        }
    }
}