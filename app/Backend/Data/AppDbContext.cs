using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.UserVault> UserVaults { get; set; }
        public DbSet<Models.Conversation> Conversations { get; set; }
        public DbSet<Models.ConversationUser> ConversationUsers { get; set; }
        public DbSet<Models.ConversationVault> ConversationVaults { get; set; }
        public DbSet<Models.MessageMetadata> MessageMetadatas { get; set; }
        public DbSet<Models.Attachment> Attachments { get; set; }
        public DbSet<Models.Prekeys> Prekeys { get; set; }
        public DbSet<Models.InviteCode> InviteCodes { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            
        }
    }
}
