namespace ZentrixLabs.PrtgSdk.Models;

public class PrtgSensor
{
    public int ObjId { get; set; }
    public string Sensor { get; set; } = string.Empty;
    public string Device { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? LastValue { get; set; }
    public DateTime? LastUpdate { get; set; }
}