using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Services.Cache
{
    public static class KeyFactory
    {
        public static string MessageKey(Guid conversationId, int messageSequence)
        {
            return $"messages:{conversationId}:{messageSequence}";
        }

        public static string MessageRecentKey(Guid conversationId)
        {
            return $"messages:recent:{conversationId}";
        }
    }
}