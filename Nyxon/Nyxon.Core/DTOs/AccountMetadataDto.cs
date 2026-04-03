using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class AccountMetadataDto
    {
        public DateTime JoinedAt { get; set; }
        public int InvitesCount { get; set; }
    }
}