using System.Drawing;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlateRecognizer.Data;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PlateRecognizer.Migrations.ApplicationDb;
using PlateRecognizer.Models;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;
using System.Configuration;
using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Drawing.Printing;
using Microsoft.EntityFrameworkCore.Storage;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Identity.Client;
using static Azure.Core.HttpHeader;

namespace PlateRecognizer
{
    public class Api
    {
        private static IConfigurationRoot configuration;


        private static ApplicationDbContext _dbContext;
        private static PlateDbContext _plateDbContext;
        private static DbFilesContext _dbFilesContext;
        private static DbImageContext _dbImageContext;
        private static AppDbContextFactory _appDbContextFactory;
        private static ApiDbContextFactory _apiDbContextFactory;
        private static DbFilesContextFactory _DbFilesContextFactory;
        private static DbImageContextFactory _DbImageContextFactory;

        public static string _dbName;
        public static string _region;

        public static double? _highScore;
        public static string _highPlate;

        public static string postUrl;
        public static string apiKey;


        static async Task Main(string[] args)
        {

            var services = new ServiceCollection();
            services.AddScoped<AppDbContextFactory>();
            services.AddScoped<ApiDbContextFactory>();
            services.AddScoped<DbFilesContextFactory>();
            services.AddScoped<DbImageContextFactory>();

            var serviceProvider = services.BuildServiceProvider();

            _appDbContextFactory = serviceProvider.GetService<AppDbContextFactory>();
            _apiDbContextFactory = serviceProvider.GetService<ApiDbContextFactory>();
            _DbFilesContextFactory = serviceProvider.GetService<DbFilesContextFactory>();
            _DbImageContextFactory = serviceProvider.GetService<DbImageContextFactory>();
            _dbFilesContext = _DbFilesContextFactory.CreateDbContext(new string[0]); //Context for Backups (DbFiles)
            _plateDbContext = _apiDbContextFactory.CreateDbContext(new string[0]);  //Context for Api Results (PlateRecognizerImage)


            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();


            postUrl = configuration.GetValue<string>("Settings:apiSettings:url");
            apiKey = configuration.GetValue<string>("Settings:apiSettings:apiKey");


            string priority = "es";
            Console.WriteLine($"Running plate for: {priority}");

            //string dbChoice = "OlxImages";
            string? dbChoice = null;
            await RunPlateRecognizer(priority, dbChoice);
        }


        static async Task MoveImagesToDbFiles(string database)
        {
            string imagesConnection = $"Data Source=192.168.120.31;Initial Catalog={database}Images;User ID=sa;Password=SQLlond2017;Enlist=false;Encrypt=false";
            _dbImageContext = _DbImageContextFactory.CreateDbContext(new[] { imagesConnection });

            bool hasImages = true;
            int batchSize = 1000;
            int processedCount = 0;

            await Console.Out.WriteLineAsync($"Moving images from {database} to DbFiles");

            while (hasImages)
            {
                var imagesBatch = _dbImageContext.Images.Take(batchSize).ToList();
                if (imagesBatch.Count == 0)
                {
                    hasImages = false;
                    continue;
                }

                foreach (var img in imagesBatch)
                {
                    _dbFilesContext.Images.Add(new DbFile()
                    {
                        ImageData = img.ImageData,
                        CarId = img.CarId,
                        IsFullPage = img.IsFullPage,
                        Project = database
                    });

                    _dbImageContext.Images.Remove(img);

                    processedCount++;
                }

                await _dbImageContext.SaveChangesAsync();
                await _dbFilesContext.SaveChangesAsync();

            

                Console.WriteLine($"Successfully processed {processedCount} images.");

                // Clearing the batch from memory
                imagesBatch.Clear();
            }

            await Console.Out.WriteLineAsync("Moved all");
        }





        static async Task RunPlateRecognizer(string priority, string? dbChoice)
        {




            if (dbChoice is not null)
            {
                await RunPlateOnDatabase(dbChoice);
            }


            List<string> databases = new List<string>();


            List<string> ptDatabases = new List<string> {
                    "StandvirtualImages",
                    "OlxImages",
                    "CustoJustoImages" ,
                    "PiscaPiscaImages",
                    "CarMineImages",
                    "AutosapoImages",
                    };

            List<string> esDatabases = new List<string> {
                    "MotorFlashImages",
                    "AutocasionImages",
                    "CochesOcasionImages"
                    };





            if (priority == "pt")
            {
                _region = "pt";
                databases = ptDatabases;
                var dbVolumesDic = GetVolumesDic(ptDatabases);
            }
            if (priority == "es")
            {
                _region = "es";
                databases = esDatabases;
                var dbVolumesDic = GetVolumesDic(esDatabases);
            }
            //else
            //{
            //    databases = GetDatabasesEndingWithImages(true);
            //    databases = OrderDatabasesByDescendingImgCount(GetVolumesDic(databases));
            //}



             
            
            while( true){

                for (int i = 0; i < databases.Count; i++)
                {
         
                    await RunPlateOnDatabase(databases[i]);
                }
            }

             


        }






        static async Task RunPlateOnDatabase(string imgDbName )
        {
                Console.WriteLine($"\nRunning PR on : {imgDbName}\n");
 
                string dbName = imgDbName.Replace("Images", "");

                if (dbName == "CochesOcasion" || dbName == "Motorflash" || dbName == "Autocasion")
                    _region = "es";
                else
                    _region = "pt";

                if (dbName == "Standvirtual" || dbName == "PiscaPisca")
                    dbName = $"{dbName}_v2";

                _dbName = dbName;
                string dbConnection = $"Data Source=192.168.120.34;Initial Catalog={dbName};User ID=sa;Password=SQLlond2017; Enlist=true;Encrypt=false";
                string imagesConnection = $"Data Source=192.168.120.31;Initial Catalog={imgDbName};User ID=sa;Password=SQLlond2017;Enlist=false;Encrypt=false";

                Console.WriteLine(dbConnection);

                _dbContext = _appDbContextFactory.CreateDbContext(new[] { dbConnection });
                _dbContext.Database.SetCommandTimeout(30);

                CheckAndAddColumns();

                _dbImageContext = _DbImageContextFactory.CreateDbContext(new[] { imagesConnection });

                try
                {
                    await ScoreImages(postUrl, apiKey);
                }
                catch (Exception e)
                {
                    _plateDbContext.Errors.Add(new Error
                    {
                        DbName = _dbName,
                        SerializedError = JsonConvert.SerializeObject(e)
                    });

                    await _plateDbContext.SaveChangesAsync();
                    throw;
                }
            }
        





        static void CheckAndAddColumns()
        {
            try
            {
                //var tableInfo = _dbContext.Database.SqlQuery<ColumnInfo>($"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cars'").ToList();
                var tableInfo = _dbContext.Database.SqlQuery<string>($"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cars'").ToList();


                if (!tableInfo.Any(info => info == "PrCount"))
                {
                    _dbContext.Database.ExecuteSqlRaw("ALTER TABLE Cars ADD PrCount INT"); 
                    Console.WriteLine("PrCount added to Cars.");
                }

                if (!tableInfo.Any(info => info == "PrScore"))
                {
                    _dbContext.Database.ExecuteSqlRaw("ALTER TABLE Cars ADD PrScore float"); 
                    Console.WriteLine("PrScore added to Cars.");
                }

                if (!tableInfo.Any(info => info == "Matricula"))
                {
                    _dbContext.Database.ExecuteSqlRaw("ALTER TABLE Cars ADD Matricula NVARCHAR(MAX)"); 
                    Console.WriteLine("Matricula added to Cars.");
                }

                if (!tableInfo.Any(info => info == "PrPlate"))
                {
                    _dbContext.Database.ExecuteSqlRaw("ALTER TABLE Cars ADD PrPlate NVARCHAR(MAX)");
                    Console.WriteLine("PrPlate added to Cars.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }


        static List<string> GetDatabasesEndingWithImages(bool isCarImagesOnly)
        {

            string connectionString = "Data Source=192.168.120.31;Initial Catalog=master;User ID=sa;Password=SQLlond2017;Enlist=false;Encrypt=false";

            string imgTypeCondition = isCarImagesOnly 
                    ? " NOT right(name, 11) = 'MotosImages' "
                    : "1 = 1";


            List<string> dbContainingImages = new List<string>();


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Query to get databases with names ending in the specified suffix
                    string query = $"  SELECT name FROM sys.databases  WHERE right(name, 6) = 'Images' AND {imgTypeCondition}   ";

                    Console.WriteLine(query);
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string dbName = reader["name"].ToString();


                                    dbContainingImages.Add(dbName);

                                
                            }
                        }
                    }

                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

            return dbContainingImages;
        }




        static Dictionary<string, int> GetVolumesDic(List<string> imgCatalogs)
        {

            Dictionary<string, int> imgVolumeDic = new Dictionary<string, int>();

            foreach (string name in imgCatalogs)
            {
                string imagesConnection = $"Data Source=192.168.120.31;Initial Catalog={name};User ID=sa;Password=SQLlond2017;Enlist=false;Encrypt=false";
                _dbImageContext = _DbImageContextFactory.CreateDbContext(new[] { imagesConnection });

                int numberOfImgs = _dbImageContext.Images.Count();
                imgVolumeDic.Add(name, numberOfImgs);
            }

            return imgVolumeDic;
        }

        static List<String> OrderDatabasesByDescendingImgCount(Dictionary<string, int> databaseVolumesDic)
        {



            var orderedList = databaseVolumesDic.OrderByDescending(kvpair => kvpair.Value);
            foreach (var kvp in orderedList)
            {
                Console.WriteLine($"Image: {kvp.Key}, Volume: {kvp.Value}");
            }


            return orderedList.Select(kvp => kvp.Key).ToList();
        }

        public static async Task UpdateDbAsync(DbContext context)
        {

            using (var dbContextTransaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                try
                {

                    await context.SaveChangesAsync();

                    await dbContextTransaction.CommitAsync();
                    context.Database.GetDbConnection().Close();

                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();

                    


                    throw;
                }
            }

        }

        public static async Task DeleteImagesByCarId(int carId)
        {
            var imagesToDelete = await _dbImageContext.Images.Where(ci => ci.CarId == carId).ToListAsync();

            // Delete the found images
            foreach (var image in imagesToDelete)
            {
                _dbImageContext.Images.Remove(image);
            }

            // Save changes to the database
            await _dbImageContext.SaveChangesAsync();
        }

        public static async Task ScoreImages(string postUrl, string apiKey, int? batchSize = 1000)
        {


            bool thereAreCarsToScore = true;
            int carCounter = 0;

            var carIds = await _dbImageContext.Images.Where(i => i.ImageData != null).Select(i => i.CarId).Distinct().ToListAsync();
            Console.WriteLine("Total: " + carIds.Count);





            while (thereAreCarsToScore && carIds.Count > 0 && carCounter < batchSize)
            {

                var x = Random.Shared.Next(0, carIds.Count);
                int id = carIds[x];
                carIds.Remove(id);


                Car? car;

                try
                {
                     car = await _dbContext.Cars.FirstOrDefaultAsync(c => c.Id == id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return;
                }

                



                if (car == null) {
                    await Console.Out.WriteLineAsync("Images have no car associated, deleting the images");
                    await DeleteImagesByCarId(id);
                    continue;
                }
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n" + car.Url);
                Console.ResetColor();

                int callCount = 0;

                var carImages = _dbImageContext.Images.Where(i =>
                    i.CarId == car.Id).ToList();


                if (carImages.Any(i => i.ImageData != null))
                {
                    bool hasHistorycPlate = car.Matricula != null && car.PrScore == null;
                    if (hasHistorycPlate)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Historic Plate: " + car.Matricula);
                        Console.ResetColor();
                    }


                    List<PlateRecognizerImage> prImages = new List<PlateRecognizerImage>();
                    bool isNotScored = car.PrScore == null;
                    bool isPlateByRules = hasHistorycPlate;
                    _highScore = 0;
                    _highPlate = "";

                    foreach (ImageFile img in carImages)
                    {

                        await _dbFilesContext.Images.AddAsync(new DbFile()
                        {
                            ImageData = img.ImageData,
                            CarId = img.CarId,
                            IsFullPage = img.IsFullPage,
                            Project = _dbName
                        });

                        _dbImageContext.Images.Remove(img);

                        if (!isPlateByRules && callCount < 6 && img.IsFullPage == false && isNotScored)
                        {
                            PlateRecognizerImage imagePlate = new PlateRecognizerImage
                            {
                                ImageId = img.Id,
                                DatabaseTable = _dbName
                            };

   
                            await ProcessImage(imagePlate, img.ImageData, postUrl, apiKey);
                            callCount++;
                            prImages.Add(imagePlate);

                            if(_region == "pt")
                                isPlateByRules = RuleVerifications(prImages);
                        }

                        await UpdateDbAsync(_dbImageContext);
                        await UpdateDbAsync(_dbFilesContext);
                    }
                }


                car.PrCount = callCount;

                car.PrScore = -1;

                if (_highPlate != "")
                {
                    car.PrScore = _highScore;
                    car.PrPlate = _highPlate;
                }
                else
                {
                    car.PrScore = -2;
                }


                try
                {
                    await UpdateDbAsync(_dbContext);
                }
                catch (Exception e)
                {
                    await Console.Out.WriteLineAsync("\nError message: \n"+e.Message);
                    await Console.Out.WriteLineAsync("\nInner exception: \n"+ e.InnerException?.ToString());
                    throw;
                }
                



            }

        }

        public static bool RuleVerifications(List<PlateRecognizerImage> carPrImages)
        {
            var lastPlate = carPrImages.LastOrDefault();
            if (lastPlate == null) return false;

            var distinctPlates = carPrImages.Where(c => c.HighPlate != null).Select(c => c.HighPlate).Distinct();

            if (lastPlate.HighScore > 0.81 && Utils.isValidPlate(matricula: lastPlate.HighPlate, _region) && lastPlate.Area >= configuration.GetValue<int>("apiSettings:areaMinima"))
            {
                Console.WriteLine("Scored > 81 and matched Regex");
                return true;
            }

            foreach (string? distinctPlate in distinctPlates)
            {
                if (carPrImages.Count(r => r.HighPlate == distinctPlate) > 1)
                {
                    Console.WriteLine("Matricula Repetida");
                    return true;
                }
            }
            return false;
        }

        static async Task ProcessImage(PlateRecognizerImage? img, byte[] data, string url, string apiKey)
        {
            try
            {
                PlateReaderResult? prr = await PlateReader.Read(url, data, _region, apiKey);
                Console.WriteLine(JsonConvert.SerializeObject(prr));
                Console.WriteLine(JsonConvert.SerializeObject(prr.Results));


                if (prr.Error != null)
                {
                    img.Resposta = JsonConvert.SerializeObject(prr.Error);
                    if (prr.Error.Contains("You have reached your quota")){
                        Console.WriteLine("Reached max API quota");
                    
                        throw new Exception("PlateReaderReachedMaxQuota");
                    }

                }


                if (prr.Results.Count == 0)
                {
                    img.Resposta = null;

                    Console.WriteLine("No Result");
                }
                else
                {
                    var results = prr.Results;
                    img.Resposta = JsonConvert.SerializeObject(results, Formatting.None);

                    foreach (Result result in results)
                    {
                        string matricula = result.Plate;
                        double areaOcr = (result.Box.Xmax - result.Box.Xmin) * (result.Box.Ymax - result.Box.Ymin);

                        img.HighScore = result.Score * 100;
                        img.HighPlate = matricula;
                        img.Area = areaOcr;


                        if(Utils.isValidPlate(matricula, _region))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(String.Concat("Result: ", matricula, " --> ", result.Score));
                            Console.ResetColor();

                            if (areaOcr >= configuration.GetValue<int>("apiSettings:areaMinima"))
                            {
                                if (result.Score > _highScore)
                                {
                                    _highScore = result.Score;
                                    _highPlate = matricula;
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.Green;


                _plateDbContext.Errors.Add(new Error
                {
                    DbName = _dbName,
                    SerializedError = JsonConvert.SerializeObject(ex)
                });

                _plateDbContext.SaveChanges();
                throw;
            }


        }
    }


}
