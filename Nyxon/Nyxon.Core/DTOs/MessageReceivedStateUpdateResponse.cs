using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class MessageReceivedStateUpdateResponse
    {
        [Required]
        [NotNull]
        public DateTime UpdatedAt { get; set; }

        public MessageReceivedStateUpdateResponse(DateTime updatedAt)
        {
            UpdatedAt = updatedAt;
        }
    }
}