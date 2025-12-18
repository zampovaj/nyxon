using System.Diagnostics.CodeAnalysis;

namespace Nyxon.Core.DTOs
{
    public class RegisterRequest
    {

        [Required]
        [Length(5, 20)]
        [NotNull]
        public string Username { get; set; } = string.Empty;
        [Required]
        [Length(12, 30)]
        [NotNull]
        public string PasswordHash { get; set; } = string.Empty; // already hashed from client
        [Required]
        [Length(12,12)]
        [NotNull]
        public string InviteCode { get; set; }

        // keys created on client
        public byte[] PublicKey { get; set; } = Array.Empty<byte>();
        public byte[] EncryptedVaultKey { get; set; } = Array.Empty<byte>();
        public byte[] EncryptedIdentityKey { get; set; } = Array.Empty<byte>();

    }
}