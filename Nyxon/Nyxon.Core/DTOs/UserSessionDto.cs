using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class UserSessionDto
    {
        [Required]
        [NotNull]
        public string UserId { get; set; }
        [Required]
        [NotNull]
        [Length(5,20)]
        public string Username { get; set; }
        [Required]
        [NotNull]
        public bool IsAuthenticated { get; set; }
    }
}