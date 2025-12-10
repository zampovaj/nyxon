using Nyxon.Core.Interfaces;
using System.Security.Cryptography;

namespace Nyxon.Core.Services.Hash
{
    public class Sha256HashService : IHashService
    {
        public string HashInviteCode(string rawCode)
        {
            if (string.IsNullOrEmpty(rawCode)) return string.Empty;

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(rawCode);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}