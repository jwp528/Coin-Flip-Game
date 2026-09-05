using System.Net;
using CoinFlipGame.Api.Services;
using CoinFlipGame.Shared.Dtos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CoinFlipGame.Api.Functions;

public sealed class ExternalAuthFunctions
{
    private readonly ExternalAuthService _externalAuth;
    private readonly PlayerAccountService _accounts;

    public ExternalAuthFunctions(ExternalAuthService externalAuth, PlayerAccountService accounts)
    {
        _externalAuth = externalAuth;
        _accounts = accounts;
    }

    [Function("ExchangeExternalLogin")]
    public async Task<HttpResponseData> Login(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "player/external/login")] HttpRequestData req)
    {
        if (!_externalAuth.Enabled)
            return await Disabled(req);

        var body = await FunctionHttp.ReadAsync<ExternalLoginRequest>(req);
        if (body is null)
            return await FunctionHttp.Json(req, HttpStatusCode.BadRequest, new { error = "Request body is required." });

        var result = await _externalAuth.LoginAsync(body, FunctionHttp.DeviceId(req), default);
        return await FunctionHttp.Json(req, result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, result);
    }

    [Function("LinkExternalIdentity")]
    public async Task<HttpResponseData> Link(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "player/external/link")] HttpRequestData req)
    {
        if (!_externalAuth.Enabled)
            return await Disabled(req);

        var player = await _accounts.AuthenticateAsync(req);
        if (player is null)
            return await FunctionHttp.Json(req, HttpStatusCode.Unauthorized, new AccountActionResponse(false, "authentication-required", "Sign in again to continue."));

        var body = await FunctionHttp.ReadAsync<LinkExternalIdentityRequest>(req);
        if (body is null)
            return await FunctionHttp.Json(req, HttpStatusCode.BadRequest, new { error = "Request body is required." });

        var result = await _externalAuth.LinkAsync(player, body, default);
        return await FunctionHttp.Json(req, result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, result);
    }

    [Function("GetExternalIdentities")]
    public async Task<HttpResponseData> GetIdentities(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "player/external/identities")] HttpRequestData req)
    {
        var player = await _accounts.AuthenticateAsync(req);
        if (player is null)
            return await FunctionHttp.Json(req, HttpStatusCode.Unauthorized, new { error = "authentication-required" });

        return await FunctionHttp.Json(req, HttpStatusCode.OK, await _externalAuth.GetIdentityStatusAsync(player, default));
    }

    [Function("UnlinkExternalIdentity")]
    public async Task<HttpResponseData> Unlink(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "player/external/identities/{identityId}")] HttpRequestData req,
        string identityId)
    {
        var player = await _accounts.AuthenticateAsync(req);
        if (player is null)
            return await FunctionHttp.Json(req, HttpStatusCode.Unauthorized, new AccountActionResponse(false, "authentication-required", "Sign in again to continue."));

        var result = await _externalAuth.UnlinkIdentityAsync(player, identityId, default);
        return await FunctionHttp.Json(req, result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, result);
    }

    [Function("GetPlayerMe")]
    public async Task<HttpResponseData> Me(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "player/me")] HttpRequestData req)
    {
        var player = await _accounts.AuthenticateAsync(req);
        if (player is null)
            return await FunctionHttp.Json(req, HttpStatusCode.Unauthorized, new { error = "authentication-required" });

        return await FunctionHttp.Json(req, HttpStatusCode.OK, await _externalAuth.GetMeAsync(player, default));
    }

    [Function("LogoutPlayer")]
    public async Task<HttpResponseData> Logout(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "player/logout")] HttpRequestData req)
    {
        var player = await _accounts.AuthenticateAsync(req);
        if (player is not null)
            await _accounts.DeleteSessionAsync(player.Session.PartitionKey);

        return await FunctionHttp.Json(req, HttpStatusCode.OK, new { success = true });
    }

    private static Task<HttpResponseData> Disabled(HttpRequestData req) =>
        FunctionHttp.Json(req, HttpStatusCode.NotFound, new { error = "external-auth-disabled" });
}
