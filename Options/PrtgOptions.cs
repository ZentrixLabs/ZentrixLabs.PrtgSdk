


// Configuration for PRTG API v1
// Note: ApiToken is sensitive. Do not log this object or serialize it without redaction.
namespace ZentrixLabs.PrtgSdk.Options;

public class PrtgOptions
{
    public required string BaseUrl { get; set; }
    public required string ApiToken { get; set; }

    public void Validate()
    {
        if (!Uri.IsWellFormedUriString(BaseUrl, UriKind.Absolute))
            throw new InvalidOperationException("Invalid BaseUrl in PRTG options.");

        if (string.IsNullOrWhiteSpace(ApiToken))
            throw new InvalidOperationException("ApiToken is missing in PRTG options.");
    }
}