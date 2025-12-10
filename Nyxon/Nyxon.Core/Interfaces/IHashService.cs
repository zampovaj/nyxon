namespace Nyxon.Core.Interfaces
{
    public interface IHashService
    {
        string HashInviteCode(string rawCode);
        string HashPassword(string password);
    }
}