

using System.Text.Json.Serialization;
using ZentrixLabs.PrtgSdk.Models;

namespace ZentrixLabs.PrtgSdk.Models.Responses;

public class PrtgSensorResponse
{
    [JsonPropertyName("sensors")]
    public List<PrtgSensor> Sensors { get; set; } = new();

    public PrtgSensorResponse SanitizeForLog()
    {
        return new PrtgSensorResponse
        {
            Sensors = this.Sensors.Select(s => s.SanitizeForLog()).ToList()
        };
    }
}