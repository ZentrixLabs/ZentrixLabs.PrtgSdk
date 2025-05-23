using System.Text.Json.Serialization;

namespace ZentrixLabs.PrtgSdk.Models
{
    public class PrtgDevice
    {
        [JsonPropertyName("objid")]
        public int ObjectId { get; set; }

        [JsonPropertyName("device")]
        public string DeviceName { get; set; } = string.Empty;

        [JsonPropertyName("group")]
        public string GroupName { get; set; } = string.Empty;

        [JsonPropertyName("probe")]
        public string? ProbeName { get; set; } // Optional
        
        public PrtgDevice SanitizeForLog()
        {
            return new PrtgDevice
            {
                ObjectId = this.ObjectId,
                DeviceName = "[REDACTED]",
                GroupName = "[REDACTED]",
                ProbeName = "[REDACTED]"
            };
        }
    }
}
