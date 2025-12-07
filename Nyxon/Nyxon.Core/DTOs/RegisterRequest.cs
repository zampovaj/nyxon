namespace Nyxon.Core.DTOs
{
    public class RegisterRequest
    {
        
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string InviteCode {get; set;}

        // keys created on client
        public byte[] PublicKey {get; set;} = Array.Empty<byte>();
        public byte[] EncryptedVaultKey {get; set;} = Array.Empty<byte>();
        public byte[] EncryptedIdentityKey {get; set;} = Array.Empty<byte>();

    }
}