using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IInboxService
    {
        List<Conversation> Conversations { get; }
        event Action OnChange;
        Task SyncInboxAsync();
        void Clear();
    }
}