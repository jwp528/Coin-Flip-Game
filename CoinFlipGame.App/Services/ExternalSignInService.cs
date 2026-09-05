using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;

namespace CoinFlipGame.App.Services;

public sealed class ExternalSignInService
{
    private readonly IJSRuntime _js;
    private readonly IConfiguration _configuration;
    private readonly NavigationManager _navigation;
    private IJSObjectReference? _module;

    public ExternalSignInService(IJSRuntime js, IConfiguration configuration, NavigationManager navigation)
    {
        _js = js;
        _configuration = configuration;
        _navigation = navigation;
    }

    public bool Enabled => _configuration.GetValue<bool>("ExternalAuth:Enabled");

    public async Task StartAsync(string mode = "login", bool createAccount = false)
    {
        if (!Enabled)
            throw new InvalidOperationException("External account sign-in is disabled.");

        _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "./js/externalAuth.js");
        await _module.InvokeVoidAsync("start", new
        {
            authorizationEndpoint = _configuration["ExternalAuth:AuthorizationEndpoint"],
            clientId = _configuration["ExternalAuth:ClientId"],
            scope = _configuration["ExternalAuth:Scope"],
            redirectUri = new Uri(new Uri(_navigation.BaseUri), "auth/callback").ToString(),
            mode,
            createAccount
        });
    }

    public async Task<ExternalCallbackResult> CompleteAsync()
    {
        _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "./js/externalAuth.js");
        return await _module.InvokeAsync<ExternalCallbackResult>("complete", new
        {
            tokenEndpoint = _configuration["ExternalAuth:TokenEndpoint"],
            clientId = _configuration["ExternalAuth:ClientId"],
            redirectUri = new Uri(new Uri(_navigation.BaseUri), "auth/callback").ToString()
        });
    }
}

public sealed record ExternalCallbackResult(bool Success, string? AccessToken, string? Mode, string? Error);
