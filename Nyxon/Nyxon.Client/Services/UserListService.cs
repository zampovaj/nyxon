using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Services
{
    public class UserListService : IUserListService
    {
        private readonly IUserRepository _repository;
        private readonly IInboxService _inboxService;

        public UserListService(IUserRepository repository, IInboxService inboxService)
        {
            _repository = repository;
            _inboxService = inboxService;
        }

        public List<UserModel> Users { get; private set; } = new();

        public event Action OnChange;

        public List<UserModel> SearchUsers(string query, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Users
                    .OrderByDescending(u => u.Conversation)
                    .ThenBy(u => u.Username)
                    .Take(limit)
                    .ToList();

            query = query.Trim();

            return Users
                .Where(u =>
                    u.Username.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(u => u.Conversation)
                .ThenBy(u => u.Username)
                .Take(limit)
                .ToList();
        }

        public async Task SyncOfflineAsync()
        {
            try
            {
                var conversationUsernames = _inboxService.Conversations
                    .Select(c => c.TargetUsername)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);


                if (Users != null)
                {
                    Users
                        .Select(u => u.Conversation = conversationUsernames
                            .Contains(u.Username))
                        .ToList();

                    NotifyStateChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"User list sync failed: {ex.Message}");
            }
        }

        public async Task SyncListAsync()
        {
            try
            {
                var userDtos = await _repository.FetchUserListAsync();

                if (userDtos == null) return;

                //hash set for O(1) lookup
                var conversationUsernames = _inboxService.Conversations
                    .Select(c => c.TargetUsername)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);


                if (userDtos != null)
                {
                    Users = userDtos
                        .Where(d => d.Username != AccountConstants.DeletedAccount)
                        .Select(d => new UserModel()
                        {
                            Username = d.Username,
                            Conversation = conversationUsernames.Contains(d.Username)
                        }).ToList();

                    NotifyStateChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"USer list sync failed: {ex.Message}");
            }
        }

        public void Clear()
        {
            Users.Clear();
            NotifyStateChanged();
        }

        public void Check()
        {
            foreach (var user in Users)
            {
                Console.WriteLine(user.Username + " " + (user.Conversation ? "true " : "false"));
            }
        }

        public void Dispose()
        {
            Clear();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}