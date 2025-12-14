using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Nyxon.Server.Services.Auth
{
    public class LoginService : ILoginService
    {
        private readonly AppDbContext _context;

        public LoginService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> LoginAsync(LoginRequest request)
        {
            // verify username and password
            var user = await _context.Users
                .Where(u => u.Username == request.Username
                && u.PasswordHash == request.PasswordHash)
                .FirstOrDefaultAsync();

            return user;
        }
    }
}