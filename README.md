# ZentrixLabs.PrtgSdk

[![NuGet Version](https://img.shields.io/nuget/v/ZentrixLabs.PrtgSdk.svg)](https://www.nuget.org/packages/ZentrixLabs.PrtgSdk/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ZentrixLabs.PrtgSdk.svg)](https://www.nuget.org/packages/ZentrixLabs.PrtgSdk/)
[![GitHub](https://img.shields.io/badge/GitHub-ZentrixLabs.PrtgSdk-blue?logo=github)](https://github.com/ZentrixLabs/PrtgSdk)

A clean, lightweight .NET 9 SDK for working with the PRTG Network Monitor API.

## Features

- 🔐 Auth via PRTG API token
- 📡 Fetch devices and sensors
- 🧠 Sensor classification and formatting
- 🚦 Handles pagination and throttling
- 🪶 Minimal dependencies, logging via `ILogger<T>`

## Security & Auth Notes

This SDK uses **PRTG API v1**, which authenticates via API token passed in the query string.

- ✅ Best used with a **dedicated, read-only PRTG user** account.
- 🔐 Ensure HTTPS is enabled on your PRTG server.
- 🚫 Do not log full request URLs — the SDK redacts API tokens in debug logs.
- ⏱️ All HTTP requests enforce a timeout and avoid blocking indefinitely.

For session-based auth with bearer tokens (API v2), use a different integration — this SDK is intentionally optimized for unattended, background service use.

## Installation

This SDK is designed for internal use but may be published to NuGet in the future.

## Configuration

```json
{
  "PRTG": {
    "BaseUrl": "https://prtg.example.com",
    "ApiToken": "your_token_here"
  }
}
```

You may also call `.Validate()` on `PrtgOptions` to ensure required fields are set and well-formed.

Bind this config using:

```csharp
builder.Services.Configure<PrtgOptions>(builder.Configuration.GetSection("PRTG"));
builder.Services.AddHttpClient<PrtgService>();
```

## Usage

```csharp
var devices = await prtgService.GetAllDevicesWithGroupsAsync();
var sensors = await prtgService.GetSensorsByDeviceIdAsync(device.ObjectId);
```

## Logging

All operations use `ILogger<T>`:
- `Debug` level for API calls, pagination, counts
- `Warning` for non-success responses

If logging full sensor or device payloads, use `.SanitizeForLog()` to redact diagnostic fields like `Message`, `Status`, or names.


---

## 🌐 More from ZentrixLabs

Explore our tools, apps, and developer blog at [zentrixlabs.net](https://zentrixlabs.net)

---

Licensed under the [MIT License](LICENSE) by ZentrixLabs.

## Contributing

Pull requests are welcome!  
Please fork the repository, make your changes, and submit a pull request.  
Ensure changes are well-tested and align with the SDK's goals of clarity, reliability, and minimalism.

If you'd like to support this project:

[![Buy Me A Coffee](https://cdn.buymeacoffee.com/buttons/default-orange.png)](https://www.buymeacoffee.com/Mainframe79)
