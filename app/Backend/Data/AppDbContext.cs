using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Set the Composite Primary Key for the junction table
            modelBuilder.Entity<ConversationUser>()
                .HasKey(cu => new { cu.ConversationId, cu.UserId });

            // 2. Configure Relationship: User -> ConversationUsers
            modelBuilder.Entity<ConversationUser>()
                .HasOne(cu => cu.User)
                .WithMany(u => u.ConversationUsers)
                .HasForeignKey(cu => cu.UserId)
                .OnDelete(DeleteBehavior.Cascade); // If User is deleted, remove them from chats

            // 3. Configure Relationship: Conversation -> ConversationUsers
            modelBuilder.Entity<ConversationUser>()
                .HasOne(cu => cu.Conversation)
                .WithMany(c => c.ConversationUsers)
                .HasForeignKey(cu => cu.ConversationId)
                .OnDelete(DeleteBehavior.Cascade); // If Conversation is deleted, remove links
        }
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
