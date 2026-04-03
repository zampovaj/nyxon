using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Services.Users
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserListDto>> GetAllUsersButMeAsync(Guid userId)
        {
            return await _context.Users
                .Where(u => u.Id != userId)
                .Select(u => new UserListDto
                {
                    Username = u.Username
                })
                .ToListAsync();
        }

    }
}