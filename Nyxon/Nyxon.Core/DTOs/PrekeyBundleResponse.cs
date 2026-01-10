using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class PrekeyBundleResponse
    {
        [Required]
        [NotNull]
        public Guid UserId { get; set; }

        [Required]
        [NotNull]
        public Guid SpkId { get; set; }
        [Required]
        [NotNull]
        public byte[] SpkPublic { get; set; }
        [Required]
        [NotNull]
        public byte[] SpkSignature { get; set; }

        public Guid? OpkId { get; set; }
        public byte[]? OpkPublic { get; set; }

        [Required]
        [NotNull]
        [MinLength(32)]
        [MaxLength(32)]
        public byte[] PublicIdentityKey { get; set; }
        [MinLength(32)]
        [MaxLength(32)]
        public byte[] PublicAgreementKey { get; set; }
    }
}