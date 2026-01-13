using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class MessagesBundleRequest
    {
        public int Count { get; set; }
        public int LastSequenceNumber { get; set; }

        public MessagesBundleRequest(int count, int lastSequenceNumber)
        {
            Count = count;
            LastSequenceNumber = lastSequenceNumber;
        }
    }
}