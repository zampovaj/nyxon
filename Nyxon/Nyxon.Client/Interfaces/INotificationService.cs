using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface INotificationService : IDisposable
    {
        event Action<string, Guid> OnMessageNotification;
        Task InitializeAsync();
    }
}