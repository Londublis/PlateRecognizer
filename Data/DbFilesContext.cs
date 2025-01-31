using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PlateRecognizer.Models;

public class DbFilesContext : DbContext
{

    public DbFilesContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<DbFile> Images { get; set; }

}

public class DbFilesContextFactory : IDesignTimeDbContextFactory<DbFilesContext>
{
    public DbFilesContext CreateDbContext(string[] args)
    {

        var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false);
        var configuration = builder.Build();

        //var conn = configuration.GetConnectionString("DbFilesConnection");
        var conn = configuration.GetSection("Settings").GetSection("DbFilesConnection").Value;
        var optionsBuilder = new DbContextOptionsBuilder<DbFilesContext>();
        optionsBuilder.UseSqlServer(conn);
        var dt = new DbFilesContext(optionsBuilder.Options);
        return dt;
    }
}
