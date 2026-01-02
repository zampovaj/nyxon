using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Services
{
    public class UserContext
    {
        public Guid? UserId { get; private set; }
        public string Username { get; private set; }
        public bool IsAuthenticated => UserId.HasValue;

        public void SetUser(Guid userId, string username)
        {
            UserId = userId;
            Username = username;
        }

        public void Clear()
        {
            UserId = null;
            Username = string.Empty;
        }
    }
}