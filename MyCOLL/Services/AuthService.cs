using Microsoft.AspNetCore.Identity;
using MyCOLL.Data;

namespace MyCOLL.Services
{
    public class AuthService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }



        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
