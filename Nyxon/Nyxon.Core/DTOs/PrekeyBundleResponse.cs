using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class PrekeyBundleResponse
    {
        public Guid UserId { get; set; }
        
        public Guid SpkId { get; set; }
        public byte[] SpkPublic { get; set; }
        public byte[] SpkSignature { get; set; }

        public Guid? OpkId { get; set; }
        public byte[]? OpkPublic { get; set; }

        public byte[] PublicIdentityKey { get; set; }
    }
}