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
        void Clear();
        void Check();
    }
}