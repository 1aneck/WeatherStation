using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace WeatherStation
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory _httpClient;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOptions<WeatherSettings> _options;

        public Worker(
            ILogger<Worker> logger,
            IHttpClientFactory httpClient, 
            IServiceScopeFactory serviceScopeFactory, 
            IOptions<WeatherSettings> options)
        {
            _logger = logger;
            _httpClient = httpClient;
            _serviceScopeFactory = serviceScopeFactory;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {   
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                string? weatherStationUrl = _options.Value.Url;
                HttpClient client = _httpClient.CreateClient();

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                try
                {
                    string? tmpXml = await client.GetStringAsync(weatherStationUrl);
                    XDocument xmlDoc = XDocument.Parse(tmpXml);

                    var sensor = xmlDoc.Descendants("sensor")
                        .Select(s => new SensorModel
                        {
                            Id = s.Element("id")?.Value,
                            Type = s.Element("type")?.Value,
                            Name = s.Element("name")?.Value,
                            Value = s.Element("value")?.Value
                        }).ToList();
                    string jsonResult = JsonSerializer.Serialize(sensor);

                    WeatherLog newLog = new WeatherLog();

                    newLog.Id = Guid.NewGuid();
                    newLog.IsAvailable = true;
                    newLog.Data = jsonResult;
                    newLog.UploadedAt = DateTimeOffset.Now;

                    dbContext.WeatherLogs.Add(newLog);
                    await dbContext.SaveChangesAsync();

                }
                catch (Exception e)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogWarning(e, "Station offline at: {time}", DateTimeOffset.Now);
                    }

                    WeatherLog newLog = new WeatherLog();

                    newLog.Id = Guid.NewGuid();
                    newLog.IsAvailable = false;
                    newLog.ErrorMessage = e.Message;
                    newLog.UploadedAt = DateTimeOffset.Now;

                    dbContext.WeatherLogs.Add(newLog);
                    await dbContext.SaveChangesAsync();
                }
                
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
