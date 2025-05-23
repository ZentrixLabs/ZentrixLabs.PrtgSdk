using System.Text.Json.Serialization;
using ZentrixLabs.PrtgSdk.Models;

namespace ZentrixLabs.PrtgSdk.Models.Responses;

public class PrtgDeviceResponse
{
    [JsonPropertyName("devices")]
    public List<PrtgDevice> Devices { get; set; } = new();
}
