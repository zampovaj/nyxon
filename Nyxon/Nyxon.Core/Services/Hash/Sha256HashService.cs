using Nyxon.Core.Interfaces;
using System.Security.Cryptography;

namespace Nyxon.Core.Services.Hash
{
    public class Sha256HashService : IHashService
    {
        public byte[] HashPassword(byte[] password)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(password);
        }
    }
}