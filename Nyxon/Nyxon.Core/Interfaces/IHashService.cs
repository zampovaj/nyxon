namespace Nyxon.Core.Interfaces
{
    public interface IHashService
    {
        byte[] HashPassword(byte[] password);
    }
}