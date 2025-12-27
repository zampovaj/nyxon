using Nyxon.Core.Version;
using Nyxon.Server.Data;
using Nyxon.Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Services.Messaging
{
    public class ConversationService : IConversationService
    {
        private readonly AppDbContext _context;

        public ConversationService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> CreateConversationAsync(Guid initiatorId, string targetUsername)
        {
            var targetUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == targetUsername);

            if (targetUser == null) 
                throw new Exception("User not found");

            if (targetUser.Id == initiatorId)
                throw new Exception("Cannot create conversation with yourself");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    Version = AppVersion.Current
                };

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();

                var user1 = new ConversationUser(conversation.Id, initiatorId);
                var user2 = new ConversationUser(conversation.Id, targetUser.Id);

                _context.ConversationUsers.Add(user1);
                _context.ConversationUsers.Add(user2);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return conversation.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ConversationSummaryDto>> GetInboxAsync(Guid userId)
        {
            return await _context.ConversationUsers
                .AsNoTracking()
                .OrderByDescending(cu => cu.Conversation.LastMessageAt) // using index
                .Select(cu => new ConversationSummaryDto
                {
                    Id = cu.ConversationId,
                    LastMessageAt = cu.Conversation.LastMessageAt,
                    
                    // unread if the chat was updated and the user hasnt seen it yet
                    HasUnreadMessages = cu.Conversation.LastMessageAt > cu.LastRead,

                    // find the other user
                    Username = cu.Conversation.ConversationUsers
                        .Where(other => other.UserId != userId)
                        .Select(other => other.User.Username)
                        .FirstOrDefault() ?? "Unknown"
                })
                .ToListAsync();
        }
    }
}