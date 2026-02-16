using Microsoft.EntityFrameworkCore;
using Serilog;
using WeatherStation;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/weather_station.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, retainedFileCountLimit: 90)
    .CreateLogger();
Log.Information("Starting app...");
try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Logging.AddSerilog();
    builder.Services.AddHostedService<Worker>();
    builder.Services.AddHttpClient();
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"));
    });
    builder.Services.Configure<WeatherSettings>(builder.Configuration.GetSection("WeatherSettings"));

    var host = builder.Build();
    host.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Aplikace neo?ekávan? spadla!");
}
finally
{
    Log.CloseAndFlush();
}

