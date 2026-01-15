using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IAuthenticationService
    {
        Task<bool> LoginAsync(string username, byte[] password);
        Task<bool> RegisterAsync(string username, byte[] password, string inviteCode, byte[] passphrase);
        Task LogoutAsync();
        Task LocalLogoutAsync();
    }
}