using Microsoft.EntityFrameworkCore;
using FunktsiooniUurimine.Models;

namespace FunktsiooniUurimine.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Models.FunktsiooniUurimine> FunktsiooniUurimised { get; set; }
    }
}