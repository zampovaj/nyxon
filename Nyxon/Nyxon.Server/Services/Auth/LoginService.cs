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
        private readonly IPasswordService _passwordService;
        private readonly ILogger<LoginService> _logger;

        public LoginService(AppDbContext context, IPasswordService passwordService, ILogger<LoginService> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _logger = logger;
        }

        public async Task<User?> LoginAsync(LoginRequest request)
        {
            // verify username and password
            var user = await _context.Users
                .Where(u => u.Username == request.Username)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogInformation("User not found");
                return null;
            }

            if (_passwordService.VerifyPassword(request.PasswordHash, user.PasswordSalt, user.PasswordHash))
            {
                _logger.LogInformation("User checks out");
                return user;
            }

            return null;
        }
    }
}