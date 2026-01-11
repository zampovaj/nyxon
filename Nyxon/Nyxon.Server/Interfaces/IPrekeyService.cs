using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IPrekeyService
    {
        Task<PrekeyBundleResponse> GetPrekeyBundle(string username);
        Task<bool> IsNewSpkNeededAsync(Guid userId);
        Task RotateSignedPrekeyAsync(Guid userId, Nyxon.Core.Models.SignedPrekey signedPrekey);
    }
}