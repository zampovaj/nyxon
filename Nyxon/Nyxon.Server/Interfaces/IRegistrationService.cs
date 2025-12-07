using Nyxon.Core.DTOs;

namespace Nyxon.Server.Interfaces
{
    public interface IRegistrationService
    {
        Task<Guid> RegisterUserAsync(RegisterRequest request);
    }
}