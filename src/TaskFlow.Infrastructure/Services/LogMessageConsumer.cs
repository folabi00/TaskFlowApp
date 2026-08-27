using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Models;
using Task = System.Threading.Tasks.Task;

namespace TaskFlow.Infrastructure.Services
{
    public class LogMessageConsumer : BackgroundService
    {
        private readonly ILogger<LogMessageConsumer> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceProvider;

        public LogMessageConsumer(
            ILogger<LogMessageConsumer> logger,
            IConfiguration configuration,
            IServiceScopeFactory serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield(); // let app startup continue (Swagger/UI can load)

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = "log-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                AllowAutoCreateTopics = true
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            consumer.Subscribe(_configuration["Kafka:Topic"]);
            _logger.LogInformation("Kafka Consumer subscribed to topic {Topic}", _configuration["Kafka:Topic"]);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(TimeSpan.FromSeconds(1));
                    if (cr is null)
                    {
                        continue;
                    }

                    _logger.LogInformation("Consumed message: {Message}", cr.Message.Value);

                    var log = JsonSerializer.Deserialize<LogEntry>(cr.Message.Value);
                    if (log is null)
                    {
                        _logger.LogWarning("Deserialized log is null. Skipping message.");
                        continue;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<IGenericRepository<LogEntry>>();
                    await db.AddEntity(log);
                    await db.SaveChanges();

                    consumer.Commit(cr);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error");
                    //await Task.Delay(5000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background service error");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
    }
}
