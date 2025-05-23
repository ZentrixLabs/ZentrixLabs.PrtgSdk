namespace ZentrixLabs.PrtgSdk.Classification;

public static class SensorClassifier
{
    /// <summary>
    /// Attempts to classify a PRTG sensor name into a known SensorType.
    /// Classification is heuristic and based on common keyword matches.
    /// </summary>
    public static SensorType Classify(string sensorName)
    {
        if (string.IsNullOrWhiteSpace(sensorName)) return SensorType.Unknown;

        var name = sensorName.ToLowerInvariant();

        if (name.Contains("cpu")) return SensorType.CPU;
        if (name.Contains("disk")) return SensorType.Disk;
        if (name.Contains("ping")) return SensorType.Ping;
        if (name.Contains("ethernet")) return SensorType.Ethernet;
        if (name.Contains("uptime")) return SensorType.Uptime;
        if (name.Contains("service")) return SensorType.Service;
        if (name.Contains("memory"))
        {
            if (name.Contains("physical")) return SensorType.MemoryPhysical;
            if (name.Contains("virtual")) return SensorType.MemoryVirtual;
            return SensorType.Memory;
        }

        return SensorType.Unknown;
    }
}
