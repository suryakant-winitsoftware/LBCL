using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Winit.Modules.Base.Model.Constants;
using Winit.Shared.CommonUtilities.Extensions;

namespace WinIt;
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly Winit.Modules.Base.BL.ILocalStorageService _localStorage;
    private readonly HttpClient _http;

    public CustomAuthStateProvider(Winit.Modules.Base.BL.ILocalStorageService localStorage, HttpClient http)
    {
        _localStorage = localStorage;
        _http = http;
    }
    //public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    //{
    //    string token = await _localStorage.GetItem<string>("token");
    //    var identity = new ClaimsIdentity();
    //    _http.DefaultRequestHeaders.Authorization = null;

    //    if (!string.IsNullOrEmpty(token))
    //    {
    //        var tokenHandler = new JwtSecurityTokenHandler();
    //        var parsedJwt = tokenHandler.ReadJwtToken(token);
    //        identity = new ClaimsIdentity(parsedJwt.Claims, "jwt");
    //        _http.DefaultRequestHeaders.Authorization =
    //            new AuthenticationHeaderValue("Bearer", token);
    //    }
    //    var user = new ClaimsPrincipal(identity);
    //    var state = new AuthenticationState(user);
    //    NotifyAuthenticationStateChanged(Task.FromResult(state));
    //    return state;
    //}
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItem<string>(LocalStorageKeys.Token);

        var identity = string.IsNullOrEmpty(token) || IsTokenExpired(token)
                ? new ClaimsIdentity()
                : new ClaimsIdentity(JWTServiceExtensions.ParseClaimsFromJwt(token), "jwt");

        var state = new AuthenticationState(new ClaimsPrincipal(identity));
        NotifyAuthenticationStateChanged(Task.FromResult(state));
        return state;
    }
    private bool IsTokenExpired(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.ValidTo < DateTime.UtcNow;
    }
}

