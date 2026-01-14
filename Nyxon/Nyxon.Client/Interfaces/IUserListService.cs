using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IUserListService : IDisposable
    {
        public List<UserModel> Users { get; }
        event Action OnChange;
        Task SyncListAsync();
        Task SyncOfflineAsync();
        void Clear();
        void Check();
        public List<UserModel> SearchUsers(string query, int limit = 10);
    }
}