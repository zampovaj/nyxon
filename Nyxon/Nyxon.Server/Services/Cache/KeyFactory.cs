namespace Nyxon.Server.Services.Cache
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

        public static string SessionId(Guid userId)
        {
            return $"user:{userId}:session";
        }

        public static string SessionId(string userId)
        {
            return $"user:{userId}:session";
        }

        public static string InviteCodeLimit(Guid userId)
        {
            return $"limit:{userId}";
        }
        public static string InviteCode(string hash)
        {
            return $"invite:{hash}";
        }
        public static string InviteCode(byte[] hash)
        {
            return $"invite:{Convert.ToBase64String(hash)}";
        }
    }
}