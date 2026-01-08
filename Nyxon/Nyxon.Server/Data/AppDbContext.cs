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
        public DbSet<RatchetSnapshot> RatchetSnapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // user vault
            modelBuilder.Entity<UserVault>()
                .HasKey(uv => uv.UserId); // set user as pk

            // user
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasOne(u => u.UserVault)
                    .WithOne(uv => uv.User)
                    .HasForeignKey<UserVault>(uv => uv.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.SignedPrekeys)
                    .WithOne(spk => spk.User)
                    .HasForeignKey(spk => spk.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.OneTimePrekeys)
                    .WithOne(opk => opk.User)
                    .HasForeignKey(opk => opk.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

            });

            // conversation user
            modelBuilder.Entity<ConversationUser>(entity =>
            {
                entity.HasKey(cu => new { cu.ConversationId, cu.UserId });

                entity.HasOne(cu => cu.User)
                    .WithMany(u => u.ConversationUsers)
                    .HasForeignKey(cu => cu.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cu => cu.Conversation)
                    .WithMany(c => c.ConversationUsers)
                    .HasForeignKey(cu => cu.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(cu => new { cu.UserId, cu.ConversationId });
            });

            // conversation vault
            modelBuilder.Entity<ConversationVault>(entity =>
            {
                entity.HasKey(cv => new { cv.ConversationId, cv.UserId });

                entity.HasOne(cv => cv.Conversation)
                    .WithMany()
                    .HasForeignKey(cv => cv.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cv => cv.User)
                    .WithMany(u => u.ConversationVaults)
                    .HasForeignKey(cv => cv.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(v => v.VaultData)
                    .HasColumnType("jsonb");
            });

            // conversations
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasIndex(c => new { c.User1Id, c.User2Id })
                    .IsUnique();

                entity.HasIndex(c => c.LastMessageAt);

                entity.HasIndex(c => new { c.Id, c.LastSequenceNumber });
            });

            // snapshots
            modelBuilder.Entity<RatchetSnapshot>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.HasOne(s => s.User)
                     .WithMany()
                     .HasForeignKey(s => s.UserId)
                     .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Conversation)
                     .WithMany()
                     .HasForeignKey(s => s.ConversationId)
                     .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(s => new { s.UserId, s.ConversationId, s.Type, s.RotationIndex })
                    .IncludeProperties(s => s.EncryptedSessionKey);

                entity.HasIndex(s => s.RotationIndex)
                    .IsUnique();

                entity.Property(s => s.Type)
                    .HasConversion<string>();

                entity.Property(s => s.RotationIndex)
                    .IsRequired();
            });

            // handshakes
            modelBuilder.Entity<Handshake>(entity =>
            {
                entity.HasKey(h => h.Id);

                // delete handshake if user is deleted
                entity.HasOne(h => h.Initiator)
                    .WithMany()
                    .HasForeignKey(h => h.InitiatorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(h => h.TargetUser)
                    .WithMany()
                    .HasForeignKey(h => h.TargetUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // delete handshake if conversatoin is deleted
                entity.HasOne(h => h.Conversation)
                    .WithMany()
                    .HasForeignKey(h => h.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                // 3. restrict spk deletion
                entity.HasOne(h => h.Spk)
                    .WithMany()
                    .HasForeignKey(h => h.SpkId)
                    .OnDelete(DeleteBehavior.Restrict);

                // restrict opk deletion
                entity.HasOne(h => h.Opk)
                    .WithMany()
                    .HasForeignKey(h => h.OpkId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(h => new { h.TargetUserId, h.CreatedAt }); // for sorting

                entity.HasIndex(h => h.ExpiresAt);
            });

            // message metadata
            modelBuilder.Entity<MessageMetadata>(entity =>
            {
                entity.HasOne(m => m.Sender)
                    .WithMany()
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Conversation)
                    .WithMany()
                    .HasForeignKey(m => m.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(m => m.KvKey).IsUnique();
            });

            // attachments
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.HasOne(a => a.Message)
                    .WithMany(m => m.Attachments)
                    .HasForeignKey(a => a.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Owner)
                    .WithMany()
                    .HasForeignKey(a => a.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

            });
        }
    }
}
