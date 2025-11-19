using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using CoinFlipGame.Api.Models;

namespace CoinFlipGame.Api.Functions;

/// <summary>
/// Azure Function for version information endpoint
/// </summary>
public class VersionFunction
{
    private readonly ILogger<VersionFunction> _logger;

    public VersionFunction(ILogger<VersionFunction> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// HTTP endpoint that returns the current application version
    /// </summary>
    [Function("Version")]
    public async Task<HttpResponseData> GetVersion(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "version")] HttpRequestData req)
    {
        _logger.LogInformation("Version endpoint called");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        
        // Add CORS headers to allow calls from the static web app
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

        var versionInfo = new
        {
            version = AppVersion.Version,
            buildTime = AppVersion.BuildTime,
            fullVersion = AppVersion.FullVersion
        };

        await response.WriteStringAsync(JsonSerializer.Serialize(versionInfo));
        return response;
    }

    /// <summary>
    /// Handle CORS preflight requests
    /// </summary>
    [Function("VersionOptions")]
    public HttpResponseData GetVersionOptions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "version")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
        
        return response;
    }
}
