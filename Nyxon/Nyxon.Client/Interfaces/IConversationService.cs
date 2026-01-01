using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    //fetch prekeys
    //calculate x3dh
    //create vault
    //encrypt vault
    //create requestdto
    //send request to server
    //if response alreadyexisted == true
        //discard everything
        //resync
    //else
        //load the created vault and conversation to active conversation state
        //proceed to send the message normally

    public interface IConversationService
    {
        Task<Guid?> CreateConversationAsync(string username);
        Task OpenConversationAsync(Guid conversationId);
    }
}