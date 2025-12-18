using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class InviteCodeDto
    {
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        [Length(12,12)]
        public string Code { get; set; }

        public InviteCodeDto() { }
    }
}