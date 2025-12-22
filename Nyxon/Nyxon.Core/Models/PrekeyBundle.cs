using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Models
{
    public class PrekeyBundle
    {
        public SignedPrekey SPK { get; set; }
        public List<OneTimePrekey> OPKs { get; set; }

        public PrekeyBundle(SignedPrekey spk, List<OneTimePrekey> opks)
        {
            SPK = spk;
            OPKs = opks;
        }
    }
}