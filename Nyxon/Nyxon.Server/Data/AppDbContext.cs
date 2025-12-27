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
        public DbSet<Handshake> Handshakes { get; set; }

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

            // handshakes
            modelBuilder.Entity<Handshake>(entity =>
            {
                entity.HasKey(h => h.Id);

                // 1. Initiator Relationship
                // If the User is deleted, the Handshake is meaningless -> Cascade
                entity.HasOne(h => h.Initiator)
                    .WithMany()
                    .HasForeignKey(h => h.InitiatorId)
                    .OnDelete(DeleteBehavior.Cascade);

                // 2. Conversation Relationship
                // If the Conversation is deleted, the Handshake is gone -> Cascade
                entity.HasOne(h => h.Conversation)
                    .WithMany()
                    .HasForeignKey(h => h.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                // 3. Signed Prekey (SPK)
                // If SPK is deleted (rotated out), we keep the handshake record but null the reference? 
                // actually SPKs are rarely deleted immediately. Let's use Restrict to be safe, 
                // or SetNull if you plan to aggressively prune keys.
                // Recommendation: Restrict (force explicit cleanup) or NoAction.
                entity.HasOne(h => h.Spk)
                    .WithMany()
                    .HasForeignKey(h => h.SpkId)
                    .OnDelete(DeleteBehavior.Restrict); // Don't delete an SPK if it's involved in a pending handshake

                // 4. One-Time Prekey (OPK) - CRITICAL
                // OPKs are deleted *immediately* upon use.
                // We MUST use SetNull here. If we used Restrict, we couldn't delete the OPK.
                // If we used Cascade, deleting the OPK would delete the whole Handshake (bad).
                entity.HasOne(h => h.Opk)
                    .WithMany()
                    .HasForeignKey(h => h.OpkId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

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
