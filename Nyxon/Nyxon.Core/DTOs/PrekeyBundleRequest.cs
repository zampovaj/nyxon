using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class PrekeyBundleRequest
    {
        [Required]
        public string Username { get; set; }
    }
}