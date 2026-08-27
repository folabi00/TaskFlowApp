using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaskFlow.Application.Interfaces;
using LogEntry = TaskFlow.Core.Models.LogEntry;

namespace TaskFlow.Infrastructure.Services
{
    public class LogMessagePublisher : IMessagePublisher<LogEntry>, IAsyncDisposable
    {
        private IProducer<string, string> _producer;
        private readonly string _topic;
        private readonly ILogger<LogMessagePublisher> _logger;
        public LogMessagePublisher(IConfiguration configuration, ILogger<LogMessagePublisher> logger)
        {
            _logger = logger;
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageTimeoutMs = default,
                AllowAutoCreateTopics = true
            };
            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
            _topic = configuration["Kafka:Topic"] ?? "request-response.logs";
        }
        public async Task PublishAsync(LogEntry entity)
        {
            CancellationToken cancellationToken = default;
            try
            {
                var json = JsonSerializer.Serialize(entity);
                var resss = await _producer.ProduceAsync(_topic, new Message<string, string> { Key = entity.TraceId, Value = json }, cancellationToken);
                _logger.LogInformation($"QUEUE STATUS :: Status: {resss.Status}, Message: {resss.Message.Value} ");

            }
            catch (ProduceException<string, string> e)
            {
                _logger.LogError($"A Produce error occured {e.Message}");
                _logger.LogError($"Failed to publish message to broker::: Pushing Message to file temporarily" );
            }
            catch (Exception e)
            {
                _logger.LogError($"An unexpected error occured {e.Message} Publishing log to file");
            }

        }

        public ValueTask DisposeAsync()
        {
            _producer.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
