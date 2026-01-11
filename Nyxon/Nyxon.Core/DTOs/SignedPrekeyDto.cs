using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class SignedPrekeyDto
    {
        public SignedPrekey SignedPrekey { get; set; }

        public SignedPrekeyDto(SignedPrekey signedPrekey)
        {
            SignedPrekey = signedPrekey;
        }
    }
}