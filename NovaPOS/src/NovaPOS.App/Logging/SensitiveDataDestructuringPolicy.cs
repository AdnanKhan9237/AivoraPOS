using Serilog.Core;
using Serilog.Events;

namespace NovaPOS.App.Logging;

public sealed class SensitiveDataDestructuringPolicy : IDestructuringPolicy
{
    private static readonly string[] SensitivePropertyNames =
    [
        "pin", "password", "cardnumber", "creditcard", "cvv", "ssn", "token", "secret"
    ];

    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
    {
        result = null!;

        if (value is not string text || string.IsNullOrEmpty(text))
        {
            return false;
        }

        result = new ScalarValue("[REDACTED]");
        return true;
    }

    public static void EnrichSensitiveProperties(LogEvent logEvent)
    {
        foreach (var property in logEvent.Properties.ToList())
        {
            if (IsSensitiveProperty(property.Key))
            {
                logEvent.AddOrUpdateProperty(new LogEventProperty(property.Key, new ScalarValue("[REDACTED]")));
            }
        }
    }

    private static bool IsSensitiveProperty(string propertyName)
    {
        var normalized = propertyName.Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

        return SensitivePropertyNames.Any(sensitive =>
            normalized.Contains(sensitive, StringComparison.OrdinalIgnoreCase));
    }
}
