using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface ISyncService
    {
        Task SyncAsync();
        void Clear();
    }
}