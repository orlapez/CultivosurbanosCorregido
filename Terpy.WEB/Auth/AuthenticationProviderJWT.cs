using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Terpy.WEB.Helpers;

namespace Terpy.WEB.Auth;

public class AuthenticationProviderJWT : AuthenticationStateProvider, ILoginService
{
    private const string TokenKey = "TERPY_TOKEN";
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private readonly AuthenticationState _anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public AuthenticationProviderJWT(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _jsRuntime.GetLocalStorage(TokenKey);
        var state = BuildAuthenticationState(token);
        if (state is not null)
        {
            return state;
        }

        _httpClient.DefaultRequestHeaders.Authorization = null;
        if (token is not null)
        {
            await _jsRuntime.RemoveLocalStorage(TokenKey);
        }

        return _anonymous;
    }

    public async Task LoginAsync(string token)
    {
        var state = BuildAuthenticationState(token);
        if (state is null)
        {
            await LogoutAsync();
            return;
        }

        await _jsRuntime.SetLocalStorage(TokenKey, token);
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.RemoveLocalStorage(TokenKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }

    private AuthenticationState? BuildAuthenticationState(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
            {
                return null;
            }

            var jwt = handler.ReadJwtToken(token);
            if (jwt.ValidTo <= DateTime.UtcNow)
            {
                return null;
            }

            var identity = new ClaimsIdentity(jwt.Claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }
}
