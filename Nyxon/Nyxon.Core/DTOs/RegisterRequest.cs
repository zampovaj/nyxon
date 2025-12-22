using System.Diagnostics.CodeAnalysis;

namespace Nyxon.Core.DTOs
{
    public class RegisterRequest
    {

        [Required]
        [MinLength(5)]
        [MaxLength(20)]
        [NotNull]
        public string Username { get; set; } = string.Empty;
        [Required]
        [NotNull]
        public string PasswordHash { get; set; } = string.Empty; // already hashed from client
        [Required]
        [NotNull]
        [MinLength(16)]
        [MaxLength(16)]
        public byte[] Salt {get; set;}
        [Required]
        [MinLength(12)]
        [MaxLength(12)]
        [NotNull]
        public string InviteCode { get; set; }

        // keys created on client
        public byte[] PublicKey { get; set; } = Array.Empty<byte>();
        public byte[] EncryptedVaultKey { get; set; } = Array.Empty<byte>();
        public byte[] EncryptedIdentityKey { get; set; } = Array.Empty<byte>();

    }
}