namespace ZentrixLabs.PrtgSdk.Models;

public class PrtgSensor
{
    public int ObjId { get; set; }
    public string Sensor { get; set; } = string.Empty;
    public string Device { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    // NOTE: Message may include internal system info (IP, diagnostics). Redact before logging.
    public string Message { get; set; } = string.Empty;
    public string? LastValue { get; set; }
    public DateTime? LastUpdate { get; set; }

    public PrtgSensor SanitizeForLog()
    {
        return new PrtgSensor
        {
            ObjId = this.ObjId,
            Sensor = this.Sensor,
            Device = this.Device,
            Group = this.Group,
            Status = "[REDACTED]",
            Message = "[REDACTED]",
            LastValue = this.LastValue,
            LastUpdate = this.LastUpdate
        };
    }
}