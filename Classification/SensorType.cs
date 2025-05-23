namespace ZentrixLabs.PrtgSdk.Classification;

/// <summary>
/// Represents known sensor classifications for PRTG.
/// Used internally for filtering, grouping, and formatting logic.
/// </summary>
public enum SensorType
{
    Unknown,
    CPU,
    MemoryPhysical,
    MemoryVirtual,
    Memory,
    Disk,
    Ping,
    Ethernet,
    Uptime,
    Service
}
