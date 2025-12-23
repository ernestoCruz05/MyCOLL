using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace MyCOLL.UIComponents.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider, IDisposable
    {
        private readonly UserService _userService;

        public CustomAuthStateProvider(UserService userService)
        {
            _userService = userService;
            _userService.OnUserChanged += StateChanged;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var identity = new ClaimsIdentity();

                if (_userService != null && _userService.IsLoggedIn && _userService.CurrentUser != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, _userService.CurrentUser.Id ?? ""),
                        new Claim(ClaimTypes.Name, _userService.CurrentUser.Name ?? ""),
                        new Claim(ClaimTypes.Email, _userService.CurrentUser.Email ?? "")
                    };

                    if (!string.IsNullOrEmpty(_userService.CurrentUser.Role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, _userService.CurrentUser.Role));
                    }

                    identity = new ClaimsIdentity(claims, "CustomAuth");
                }

                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
            }
            catch
            {
                // Em caso de erro, devolve não autenticado para não crashar a app
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }
        }

        private void StateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void Dispose()
        {
            if (_userService != null)
            {
                _userService.OnUserChanged -= StateChanged;
            }
        }
    }
}