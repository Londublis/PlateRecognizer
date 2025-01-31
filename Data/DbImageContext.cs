using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PlateRecognizer.Models;

namespace PlateRecognizer.Data
{
    public class DbImageContext : DbContext
    {

        public DbImageContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<ImageFile> Images { get; set; }

        //public DbSet<ImageFile> Images_Old { get; set; }

    }

    public class DbImageContextFactory : IDesignTimeDbContextFactory<DbImageContext>
    {
        public DbImageContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbImageContext>();
            
            optionsBuilder.UseSqlServer(args[0]);
            var dt = new DbImageContext(optionsBuilder.Options);
            dt.Database.SetCommandTimeout(TimeSpan.FromMinutes(2)); 
            return dt;
        }
    }
}
