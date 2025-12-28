using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Models
{
    public class X3DHResult
    {
        public byte[] PublicEphemeralKey { get; set; }
        public byte[] SharedSecret { get; set; }

        public X3DHResult() { }

        public X3DHResult(byte[] publicEphemeralKey, byte[] sharedSecret)
        {
            PublicEphemeralKey = publicEphemeralKey;
            SharedSecret = sharedSecret;
        }
    }
}