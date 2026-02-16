using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Serilog;
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
                var time = DateTimeOffset.Now;
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                string? weatherStationUrl = _options.Value.Url;
                HttpClient client = _httpClient.CreateClient();

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {Time}", time);
                }

                var log = new WeatherLog
                {
                    Id = Guid.NewGuid(),
                    UploadedAt = time
                };

                try
                {
                    string? tmpXml = await client.GetStringAsync(weatherStationUrl, stoppingToken);
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

                    log.IsAvailable = true;
                    log.Data = jsonResult;

                }
                catch (Exception e)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogWarning(e,"Station offline at: {time}", time);
                    }

                    log.IsAvailable = false;
                    log.ErrorMessage = e.Message;
                }
                finally
                {
                    dbContext.WeatherLogs.Add(log);
                    await dbContext.SaveChangesAsync();
                }
                
                await Task.Delay(_options.Value.Pause, stoppingToken);
            }
        }
    }
}
