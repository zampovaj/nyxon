using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Services
{
    public static class AadFactory
    {
        private const string IdentityPrefix = "nyxon::identity::v1";
        private const string MessagePrefix = "nyxon::message::v1";
        private const string SessionReceivePrefix = "nyxon::session::recv::v1";
        private const string SessionSendPrefix = "nyxon::session::send::v1";
        private const string UserVaultPrefix = "nyxon::user_vault::v1";
        private const string RootKeyPrefix = "nyxon::root_key::v1";
        

        public static byte[] ForIdentityKey(Guid userId)
        {
            return GetBytes($"{IdentityPrefix}::{userId}");
        }

        public static byte[] ForMessage(Guid conversationId, int messageSequence)
        {
            return GetBytes($"{MessagePrefix}::{conversationId}::{messageSequence}");
        }

        public static byte[] ForReceivingSessionKey(Guid conversationId, int rotationIndex)
        {
            return GetBytes($"{SessionReceivePrefix}::{conversationId}::{rotationIndex}");
        }

        public static byte[] ForSendingSessionKey(Guid conversationId, int rotationIndex)
        {
            return GetBytes($"{SessionSendPrefix}::{conversationId}::{rotationIndex}");
        }

        public static byte[] ForUserVaultKey(Guid userId)
        {
            return GetBytes($"{UserVaultPrefix}::{userId}");
        }

        public static byte[] ForRootKey(Guid conversationId, Guid userId)
        {
            return GetBytes($"{RootKeyPrefix}::{conversationId}::{userId}");
        }

        private static byte[] GetBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
    }
}