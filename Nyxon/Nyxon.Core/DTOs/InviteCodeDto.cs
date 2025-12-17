using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class InviteCodeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }

        public InviteCodeDto() { }
    }
}