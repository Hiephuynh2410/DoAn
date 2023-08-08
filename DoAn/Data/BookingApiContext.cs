using Microsoft.EntityFrameworkCore;
using DoAn.Models;
namespace DoAn.Data
{
    public class BookingApiContext : DbContext
    {
        public BookingApiContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Cilent> clients { get; set;}
        public DbSet<Product> products { get; set; }

    }
}
