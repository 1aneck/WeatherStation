using Microsoft.EntityFrameworkCore;
using WeatherStation;

var builder = Host.CreateApplicationBuilder(args);
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
