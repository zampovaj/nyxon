using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Models
{
    public class UserModel
    {
        public string Username { get; set; } = "";
        public bool Conversation { get; set; } = false;

        public UserModel() { }
        public UserModel(string username, bool conversation)
        {
            Username = username;
            Conversation = conversation;
        }
    }
}