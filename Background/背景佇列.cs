using System.Text.Json;
using System;
using System.Threading.Channels;
using TryMinimalAPIs.Models;

namespace TryMinimalAPIs.Background
{
    public class 背景佇列 : BackgroundService
    {
        private readonly Channel<ReadOnlyMemory<byte>> queue;
        private readonly ILogger<背景佇列> logger;

        public 背景佇列(Channel<ReadOnlyMemory<byte>> queue,
                               ILogger<背景佇列> logger)
        {
            this.queue = queue;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var dataStream in queue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    var todo = JsonSerializer.Deserialize<TodoRequest>(dataStream.Span)!;
                    logger.LogInformation($"{todo.Id}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                }
            }
        }
    }
}
