using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class NewInviteCodesRequest
    {
        public int Count { get; set; } = 1;

        public NewInviteCodesRequest(int count)
        {
            Count = count;
        }
    }
}