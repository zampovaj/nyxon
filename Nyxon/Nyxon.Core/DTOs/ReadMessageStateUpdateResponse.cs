using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class ReadMessageStateUpdateResponse
    {
        [Required]
        [NotNull]
        public DateTime UpdatedAt { get; set; }

        public ReadMessageStateUpdateResponse(DateTime updatedAt)
        {
            UpdatedAt = updatedAt;
        }
    }
}