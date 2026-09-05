using System.Net;
using CoinFlipGame.Api.Services;
using CoinFlipGame.Shared.Dtos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CoinFlipGame.Api.Functions;

public sealed class PlayerProgressFunctions
{
    private readonly PlayerAccountService _accounts;
    private readonly PlayerProgressService _progress;

    public PlayerProgressFunctions(PlayerAccountService accounts, PlayerProgressService progress)
    {
        _accounts = accounts;
        _progress = progress;
    }

    [Function("GetPlayerProgress")]
    public async Task<HttpResponseData> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "player/progress")] HttpRequestData req)
    {
        var player = await _accounts.AuthenticateAsync(req);
        if (player is null)
            return await FunctionHttp.Json(req, HttpStatusCode.Unauthorized, new { error = "authentication-required" });

        var dto = await _progress.GetAsync(player.Account.PartitionKey);
        return await FunctionHttp.Json(req, HttpStatusCode.OK, dto);
    }

    [Function("PutPlayerProgress")]
    public async Task<HttpResponseData> Put(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "player/progress")] HttpRequestData req)
    {
        var player = await _accounts.AuthenticateAsync(req);
        if (player is null)
            return await FunctionHttp.Json(req, HttpStatusCode.Unauthorized, new { error = "authentication-required" });

        var body = await FunctionHttp.ReadAsync<PlayerProgressDto>(req);
        if (body is null)
            return await FunctionHttp.Json(req, HttpStatusCode.BadRequest, new { error = "Request body is required." });

        var merged = await _progress.MergeAndSaveAsync(player.Account.PartitionKey, body);
        return await FunctionHttp.Json(req, HttpStatusCode.OK, merged);
    }
}
