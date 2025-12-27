using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class CreateConversationResponse
    {
        public Guid ConversationId { get; set; }
        // in case bob and alice create a conversaton at the saem time
        // bob could send a handshake request
        // when alice has already just created a conversation and a handshake in db
        // in that case
        // bobs request gets discarded
        // so do data in bobs memory
        // bob fetches handshake from alice
        // and synchronizes accordingly
        public bool AlreadyExisted { get; set; }
    }
}