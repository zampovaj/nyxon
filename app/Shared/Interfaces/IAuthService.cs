namespace Shared.Interfaces
{
    public interface IAuthService
    {
        Task<Guid> RegisterUserAsync(RegisterRequest request);

    }
}