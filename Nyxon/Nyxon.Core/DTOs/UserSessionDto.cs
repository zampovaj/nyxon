using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class UserSessionDto
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}