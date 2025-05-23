using ZentrixLabs.PrtgSdk.Models;
using ZentrixLabs.PrtgSdk.Classification;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ZentrixLabs.PrtgSdk.Formatting;

public static class SensorFormatter
{
    public static string FormatLabel(PrtgSensor sensor)
    {
        var type = SensorClassifier.Classify(sensor.Sensor);
        return type switch
        {
            SensorType.CPU => "CPU Usage",
            SensorType.MemoryPhysical => "Physical Memory Available",
            SensorType.MemoryVirtual => "Virtual Memory Available",
            SensorType.Memory => "Memory Available",
            SensorType.Disk => "Disk Free Space",
            SensorType.Ping => "Ping Time",
            SensorType.Ethernet => "Ethernet Traffic",
            SensorType.Uptime => "System Uptime",
            _ => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(sensor.Sensor.Trim()) // fallback: prettify raw name
        };
    }

    public static bool ShouldShowValue(PrtgSensor sensor)
    {
        var type = SensorClassifier.Classify(sensor.Sensor);
        return type != SensorType.Service && type != SensorType.Unknown;
    }

    public static bool ShouldShowMessage(PrtgSensor sensor)
    {
        var type = SensorClassifier.Classify(sensor.Sensor);
        return type == SensorType.Uptime ||
               (!string.Equals(sensor.Status, "Up", StringComparison.OrdinalIgnoreCase) &&
               !string.IsNullOrWhiteSpace(sensor.Message));
    }

    public static string GetParsedValue(PrtgSensor sensor)
    {
        var type = SensorClassifier.Classify(sensor.Sensor);
        var raw = sensor.LastValue ?? "";

        return type switch
        {
            SensorType.CPU => $"{ExtractFirstNumber(raw)}%",
            SensorType.Ping => $"{ExtractFirstNumber(raw)} ms",
            SensorType.Memory or SensorType.MemoryPhysical or SensorType.MemoryVirtual or SensorType.Disk or SensorType.Ethernet
                => StripHtml(raw),
            _ => ExtractFirstNumber(raw) ?? StripHtml(raw)
        };
    }

    public static string FormatMessage(PrtgSensor sensor)
    {
        return StripHtml(sensor.Message);
    }

    private static string? ExtractFirstNumber(string input)
    {
        var match = Regex.Match(input, @"([\d\.]+)");
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string StripHtml(string? input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? string.Empty
            : Regex.Replace(input, "<.*?>", "").Trim();
    }
}
