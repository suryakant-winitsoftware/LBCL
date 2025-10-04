using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly Winit.Modules.Base.BL.ILocalStorageService _localStorage;
        private readonly HttpClient _http;

        public CustomAuthStateProvider(Winit.Modules.Base.BL.ILocalStorageService localStorage, HttpClient http)
        {
            _localStorage = localStorage;
            _http = http;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            Console.WriteLine("auth started");
            string token = await _localStorage.GetItem<string>("token");
            Console.WriteLine($"token is {token}");
            //Console.Write(token);
            var identity = new ClaimsIdentity();
            _http.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"1");

                var tokenHandler = new JwtSecurityTokenHandler();
                var parsedJwt = tokenHandler.ReadJwtToken(token);
                identity = new ClaimsIdentity(parsedJwt.Claims, "jwt");
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
                Console.WriteLine($"2");

                // Remove existing Source header if present
                if (_http.DefaultRequestHeaders.Contains("Source"))
                {
                    _http.DefaultRequestHeaders.Remove("Source");
                }
                // Add the Source header
                _http.DefaultRequestHeaders.Add("Source", "App");
            }

            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(state));
            Console.WriteLine($"3");
            return state;
        }
    }

}
