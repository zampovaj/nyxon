namespace Backend.Interfaces
{
    public interface IRegistrationService
    {
        Task<Guid> RegisterUserAsync(RegisterRequest request);
    }
}