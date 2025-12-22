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
        public byte[] PasswordSalt { get; set; }
        [Required]
        [NotNull]
        [MinLength(32)]
        [MaxLength(32)]
        public byte[] PassphraseSalt { get; set; }
        [Required]
        [MinLength(12)]
        [MaxLength(12)]
        [NotNull]
        public string InviteCode { get; set; }

        // keys created on client

        [Required]
        [NotNull]
        public byte[] PublicIdentityKey { get; set; } = Array.Empty<byte>();
        [Required]
        [NotNull]
        public byte[] EncryptedVaultKey { get; set; } = Array.Empty<byte>();
        [Required]
        [NotNull]
        public byte[] EncryptedPrivateIdentityKey { get; set; } = Array.Empty<byte>();

        [Required]
        [NotNull]
        public PrekeyBundle PrekeyBundle { get; set; }

    }
}