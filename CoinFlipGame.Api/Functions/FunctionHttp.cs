using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace CoinFlipGame.Api.Functions;

internal static class FunctionHttp
{
    public const string UserIdHeader = "X-User-Id";

    public static async Task<HttpResponseData> Json<T>(HttpRequestData req, HttpStatusCode status, T body)
    {
        var response = req.CreateResponse(status);
        AddCors(response);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(body, JsonDefaults.Options));
        return response;
    }

    public static HttpResponseData Status(HttpRequestData req, HttpStatusCode status)
    {
        var response = req.CreateResponse(status);
        AddCors(response);
        return response;
    }

    public static void AddCors(HttpResponseData response)
    {
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-User-Id");
    }

    public static async Task<T?> ReadAsync<T>(HttpRequestData req)
    {
        try
        {
            return await JsonSerializer.DeserializeAsync<T>(req.Body, JsonDefaults.Options);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public static string Header(HttpRequestData req, string name)
    {
        return req.Headers.TryGetValues(name, out var values)
            ? values.FirstOrDefault() ?? string.Empty
            : string.Empty;
    }

    public static string DeviceId(HttpRequestData req)
    {
        var id = Header(req, UserIdHeader);
        return string.IsNullOrWhiteSpace(id) ? "unknown" : id.Trim();
    }
}
