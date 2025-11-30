using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Version;

/*id : uuid pk ai
username : string
password_hash :  string
public_key
created_at : datetime
version : small int
admin : bool
can_create_invites : bool*/

namespace Backend.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public byte[] PublicKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public short Version { get; set; }
        public bool Admin { get; set; }
        public bool CanCreateInvites { get; set; }

        public User(Guid id, string username, string passwordHash, byte[] publicKey, DateTime createdAt, short version, bool admin, bool canCreateInvites)
        {
            Id = id;
            Username = username;
            PasswordHash = passwordHash;
            PublicKey = publicKey;
            CreatedAt = createdAt;
            Version = version;
            Admin = admin;
            CanCreateInvites = canCreateInvites;
        }

        /// <summary>
        /// Creates a brand new user
        /// </summary>
        public User(string username, string passwordHash, byte[] publicKey, bool admin, bool canCreateInvites)
        {
            Id = Guid.NewGuid();
            Username = username;
            PasswordHash = passwordHash;
            PublicKey = publicKey;
            CreatedAt = DateTime.UtcNow;
            Version = AppVersion.Current;
            Admin = admin;
            CanCreateInvites = canCreateInvites;
        }
    }
}