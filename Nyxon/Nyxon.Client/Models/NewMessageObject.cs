using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Models
{
    public class NewMessageObject
    {
        
        public Guid Id { get; set; }
        public int SequenceNumber { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }

        // ui flags
        public bool IsMine { get; set; }
    }
}