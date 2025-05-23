using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZentrixLabs.PrtgSdk.Models.Responses;
using ZentrixLabs.PrtgSdk.Models;
using ZentrixLabs.PrtgSdk.Options;

namespace ZentrixLabs.PrtgSdk.Services
{
    public class PrtgService
    {
        private readonly HttpClient _httpClient;
        private readonly PrtgOptions _options;
        private readonly ILogger<PrtgService> _logger;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);

        private readonly string _baseUrl;
        private readonly string _apiToken;

        public PrtgService(HttpClient httpClient, IOptions<PrtgOptions> options, ILogger<PrtgService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;

            _baseUrl = _options.BaseUrl ?? throw new InvalidOperationException("PRTG:BaseUrl missing");
            _apiToken = _options.ApiToken ?? throw new InvalidOperationException("PRTG:ApiToken missing");
            _logger = logger;
        }

        public async Task<int?> GetDeviceIdByNameAsync(string deviceName)
        {
            var url = $"{_baseUrl}/api/table.json?content=devices&output=json&columns=objid&filter_device={Uri.EscapeDataString(deviceName)}&apitoken={_apiToken}";
            _logger.LogDebug("[DEBUG] Fetching device ID for {DeviceName}, URL: {Url}", deviceName, url);

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[DEBUG] Failed to fetch device ID for {DeviceName}, Status: {StatusCode}", deviceName, response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<PrtgDeviceResponse>();
            var deviceId = result?.Devices.FirstOrDefault()?.ObjectId;
            _logger.LogDebug("[DEBUG] Device ID for {DeviceName}: {DeviceId}", deviceName, deviceId ?? -1);
            return deviceId;
        }

        public async Task<List<PrtgSensor>?> GetSensorsByDeviceNameAsync(string deviceName)
        {
            var deviceId = await GetDeviceIdByNameAsync(deviceName);
            if (deviceId == null)
            {
                _logger.LogWarning("[DEBUG] Device ID not found for {DeviceName}", deviceName);
                return null;
            }

            var url = $"{_baseUrl}/api/table.json?content=sensors&output=json&columns=objid,sensor,status,message,lastvalue&filter_parentid={deviceId}&apitoken={_apiToken}";
            _logger.LogDebug("[DEBUG] Fetching sensors for {DeviceName} (ID: {DeviceId}), URL: {Url}", deviceName, deviceId, url);

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[DEBUG] Failed to fetch sensors for {DeviceName}, Status: {StatusCode}", deviceName, response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<PrtgSensorResponse>();
            var sensors = result?.Sensors;
            _logger.LogDebug("[DEBUG] Fetched {SensorCount} sensors for {DeviceName}", sensors?.Count ?? 0, deviceName);
            return sensors;
        }

        // 🔍 NEW: Discover devices grouped by PRTG group name (used as "site")
        public async Task<List<PrtgDevice>> GetAllDevicesWithGroupsAsync()
        {
            var url = $"{_baseUrl}/api/table.json?content=devices&output=json&columns=objid,device,group&apitoken={_apiToken}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PrtgDeviceResponse>(json);
            return result?.Devices ?? new();
        }

        public async Task<List<string>> GetDeviceNamesInGroupAsync(int groupId)
        {
            var url = $"{_baseUrl}/api/table.json?content=devices&output=json&columns=device,group,objid&filter_parentid={groupId}&apitoken={_apiToken}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<string>();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PrtgDeviceResponse>(json);
            return result?.Devices
                .Where(d => !string.IsNullOrWhiteSpace(d.DeviceName))
                .Select(d => d.DeviceName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<string>();
        }

        public async Task<List<PrtgSensor>> GetSensorsByDeviceIdAsync(int deviceId)
        {
            var sensors = new List<PrtgSensor>();
            int start = 0;
            const int pageSize = 500;

            while (true)
            {
                var url = $"{_baseUrl}/api/table.json?content=sensors&output=json&columns=objid,sensor,status,message,lastvalue&filter_parentid={deviceId}&start={start}&count={pageSize}&apitoken={_apiToken}";
                _logger.LogDebug("[DEBUG] Fetching sensors page starting at {Start} for device ID {DeviceId}, URL: {Url}", start, deviceId, url);

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[DEBUG] Failed to fetch sensors page starting at {Start} for device ID {DeviceId}, Status: {StatusCode}", start, deviceId, response.StatusCode);
                    break;
                }

                var result = await response.Content.ReadFromJsonAsync<PrtgSensorResponse>();
                if (result?.Sensors == null || result.Sensors.Count == 0)
                {
                    break;
                }

                sensors.AddRange(result.Sensors);

                if (result.Sensors.Count < pageSize)
                {
                    break;
                }

                start += pageSize;
            }

            _logger.LogDebug("[DEBUG] Total sensors fetched for device ID {DeviceId}: {Count}", deviceId, sensors.Count);
            return sensors;
        }

        /// <summary>
        /// Bulk fetch sensors for a list of PrtgDevice, limiting concurrency to avoid hammering the PRTG API.
        /// Returns a dictionary keyed by device ID.
        /// </summary>
        /// <param name="devices">List of PrtgDevice</param>
        /// <param name="maxConcurrency">Maximum concurrent requests (default: 5)</param>
        public async Task<Dictionary<int, List<PrtgSensor>>> GetAllSensorsByDevicesAsync(
            IEnumerable<PrtgDevice> devices,
            int maxConcurrency = 5)
        {
            if (devices == null) throw new ArgumentNullException(nameof(devices));
            var result = new Dictionary<int, List<PrtgSensor>>();
            var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task>();
            var locker = new object();

            foreach (var device in devices)
            {
                if (device == null || device.ObjectId == 0) continue;
                await semaphore.WaitAsync();
                var deviceId = device.ObjectId;
                var task = Task.Run(async () =>
                {
                    try
                    {
                        var sensors = await GetSensorsByDeviceIdAsync(deviceId);
                        lock (locker)
                        {
                            result[deviceId] = sensors ?? new List<PrtgSensor>();
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
            return result;
        }

    }


}