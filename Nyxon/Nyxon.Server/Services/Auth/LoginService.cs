using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Core.DTOs;
using Nyxon.Server.Data;
using Nyxon.Server.Interfaces;

namespace Nyxon.Server.Services.Auth
{
    public class LoginService : ILoginService
    {
        private readonly IJwtService _jwtService;
        private readonly AppDbContext _context;

        public LoginService(IJwtService jwtService, AppDbContext context)
        {
            _jwtService = jwtService;
            _context = context;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            // verify username and password
            var user = await _context.Users
                .Where(u => u.Username == request.Username
                && u.PasswordHash == request.PasswordHash)
                .FirstOrDefaultAsync();

            if (user == null) return null;

            // generate token
            var token = _jwtService.GenerateToken(user.Id, user.Username);

            return new LoginResponse
            {
                UserId = user.Id,
                Token = token
            };
        }
    }
}