using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PlateRecognizer.Models;

namespace PlateRecognizer.Data
{
    public class PlateDbContext : DbContext
    {

        public PlateDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<PlateRecognizerImage> PlateResults { get; set; }
        public DbSet<Error> Errors { get; set; }
    }

    public class ApiDbContextFactory : IDesignTimeDbContextFactory<PlateDbContext>
    {
        public PlateDbContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false);
            var configuration = builder.Build();

            //var conn = configuration.GetConnectionString("PlateDbConnection");
            var conn = configuration.GetSection("Settings").GetSection("PlateDbConnection").Value;

            var optionsBuilder = new DbContextOptionsBuilder<PlateDbContext>();
            optionsBuilder.UseSqlServer(conn);
            var dt = new PlateDbContext(optionsBuilder.Options);
            return dt;
        }
    }
}
