using Microsoft.EntityFrameworkCore;
using Nyxon.Server.Models;

namespace Nyxon.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserVault> UserVaults { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationUser> ConversationUsers { get; set; }
        public DbSet<ConversationVault> ConversationVaults { get; set; }
        public DbSet<MessageMetadata> MessageMetadata { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Nyxon.Server.Models.SignedPrekey> SignedPrekeys { get; set; }
        public DbSet<Nyxon.Server.Models.OneTimePrekey> OneTimePrekeys { get; set; }
        public DbSet<InviteCode> InviteCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1. User & UserVault (1:1) ---
            // Combined Key definition + Relationship + Cascade Delete
            modelBuilder.Entity<UserVault>()
                .HasKey(uv => uv.UserId); // Explicitly set UserId as PK

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserVault)
                .WithOne(uv => uv.User)
                .HasForeignKey<UserVault>(uv => uv.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Important: Delete Vault when User is deleted

            // --- 2. User & SignedPrekeys (1:N) ---
            modelBuilder.Entity<User>()
                .HasMany(u => u.SignedPrekeys)
                .WithOne(spk => spk.User)
                .HasForeignKey(spk => spk.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- 3. User & OneTimePrekeys (1:N) ---
            modelBuilder.Entity<User>()
                .HasMany(u => u.OneTimePrekeys)
                .WithOne(opk => opk.User)
                .HasForeignKey(opk => opk.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- 3. ConversationUser (M:N Junction) ---
            modelBuilder.Entity<ConversationUser>()
                .HasKey(cu => new { cu.ConversationId, cu.UserId });

            modelBuilder.Entity<ConversationUser>()
                .HasOne(cu => cu.User)
                .WithMany(u => u.ConversationUsers)
                .HasForeignKey(cu => cu.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConversationUser>()
                .HasOne(cu => cu.Conversation)
                .WithMany(c => c.ConversationUsers)
                .HasForeignKey(cu => cu.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- 4. ConversationVault (Composite Key & Relationships) ---
            modelBuilder.Entity<ConversationVault>()
                .HasKey(cv => new { cv.ConversationId, cv.UserId });

            modelBuilder.Entity<ConversationVault>()
                .HasOne(cv => cv.Conversation)
                .WithMany()
                .HasForeignKey(cv => cv.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConversationVault>()
                .HasOne(cv => cv.User)
                .WithMany(u => u.ConversationVaults)
                .HasForeignKey(cv => cv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- 5. MessageMetadata Relationships ---
            modelBuilder.Entity<MessageMetadata>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MessageMetadata>()
                .HasOne(m => m.Conversation)
                .WithMany()
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- 6. Attachment Relationships ---
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Message)
                .WithMany(m => m.Attachments)
                .HasForeignKey(a => a.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Owner)
                .WithMany()
                .HasForeignKey(a => a.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
