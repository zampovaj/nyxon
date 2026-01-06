using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class SendMessageResponse
    {
        public Guid Id { get; set; }
        public int MessageSequence { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}