using Nyxon.Core.DTOs;

namespace Nyxon.Server.Interfaces
{
    public interface IRegistrationService
    {
        Task<bool> RegisterUserAsync(RegisterRequest request);
    }
}