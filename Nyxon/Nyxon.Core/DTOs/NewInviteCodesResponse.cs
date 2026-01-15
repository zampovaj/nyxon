using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class NewInviteCodesResponse
    {
        public List<string> Codes { get; set; } = new();

        public NewInviteCodesResponse(List<string> codes)
        {
            Codes = codes;
        }
    }
}