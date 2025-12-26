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
                    Users = userDtos.Select(d => new UserModel()
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

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}