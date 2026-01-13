using Microsoft.Extensions.Logging;

namespace Inventory.Infrastructure.Logging;

public class LoggerService
{
    private readonly ILogger<LoggerService> _logger;

    public LoggerService(ILogger<LoggerService> logger)
    {
        _logger = logger;
    }

    public void LogInfo(string message)
    {
        _logger.LogInformation($"{DateTime.UtcNow}: {message}");
    }

    public void LogError(string message)
    {
        _logger.LogError($"{DateTime.UtcNow}: {message}");
    }
}
